using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.Components;
using AlexaController.Alexa.Presentation.APL.VectorGraphics;
using AlexaController.Api.UserEvent.TouchWrapper.Press;
using AlexaController.EmbyAplDataSource.DataSourceProperties;
using AlexaController.Session;
using Parallel = AlexaController.Alexa.Presentation.APL.Commands.Parallel;

namespace AlexaController.EmbyApl
{
    public abstract class Layouts
    {
        public static async Task<List<IComponent>> RenderLayoutComponents<T>(Properties<T> properties, IAlexaSession session) where T : class
        {
            switch (properties?.documentType)
            {
                case RenderDocumentType.GENERIC_VIEW_TEMPLATE: return await RenderGenericViewLayout();

                case RenderDocumentType.MEDIA_ITEM_DETAILS_TEMPLATE: return await RenderItemDetailsLayout(properties as Properties<MediaItem>, session);

                case RenderDocumentType.MEDIA_ITEM_LIST_SEQUENCE_TEMPLATE: return await RenderItemListSequenceLayout(properties as Properties<MediaItem>, session);

                case RenderDocumentType.ROOM_SELECTION_TEMPLATE: return await RenderRoomSelectionLayout(session);

                case RenderDocumentType.HELP_TEMPLATE: return await RenderHelpViewLayout();

                default: return null;
            }
        }
        private static async Task<List<IComponent>> RenderItemListSequenceLayout(Properties<MediaItem> properties, IAlexaSession session)
        {
            var layout = new List<IComponent>();
            var mediaItems = properties?.items;
            var type = mediaItems?[0].type;

            layout.Add(new Container()
            {
                id = "primary",
                width = "100vw",
                height = "100vh",
                items = new List<IComponent>()
                {
                    new Image()
                    {
                        height       = "100%",
                        overlayColor = "rgba(0,0,0,0.65)",
                        width        = "100%",
                        scale        = "best-fill",
                        position     = "absolute",
                        source       = "${payload.templateData.properties.url}${payload.templateData.properties.item.backdropImageSource}",
                    },
                    new AlexaHeader()
                    {
                        //TODO: Do we really want a header title if there is an attribution image?
                        //headerTitle            = "${payload.templateData.properties.item.name}",
                        headerBackButton       = session.paging.canGoBack,
                        headerDivider          = true,
                        headerAttributionImage = "${payload.templateData.properties.url}${payload.templateData.properties.item.logoImageSource}"
                    },
                    new Sequence()
                    {
                        height                 = "100vh",
                        width                  = "95vw",
                        left                   = "5vw",
                        scrollDirection        = "horizontal",
                        data                   = "${payload.templateData.properties.items}",
                        items                  = new List<IComponent>()
                        {
                            new TouchWrapper()
                            {
                                paddingTop = "8vh",
                                id         = "${data.id}",
                                speech     = "${payload.templateData.properties.buttonPressEffect.url}",
                                onPress    = new Parallel()
                                {
                                   commands = new List<ICommand>()
                                   {
                                       new SpeakItem() { componentId = "${data.id}" },
                                       new Command()   { type = nameof(Animations.ScaleInOutOnPress) },
                                       new SendEvent() { arguments = GetSequenceItemsOnPressArguments(type, session) }
                                   }
                                },
                                items = new List<IComponent>()
                                {
                                   await RenderComponent_SequencePrimaryImageContainer(type)
                                }
                            }
                        }
                    },
                    new AlexaFooter()
                    {
                        hintText = "",
                        position = "absolute",
                        bottom   = "1vh",
                        id       = "hint",
                        onMount  = new List<ICommand>()
                        {
                            new Sequential()
                            {
                                repeatCount = 15,
                                commands = new List<ICommand>()
                                {
                                    await Animations.FadeOut("hint", 1020, 5000),
                                    new SetValue()
                                    {
                                        componentId = "hint",
                                        property    = "hintText",
                                        value       =  "Try \"Alexa, Show The ${payload.templateData.properties.items[0].type}: " +
                                                       "${payload.templateData.properties.items[Time.seconds(localTime/payload.templateData.properties.items.length) " +
                                                       "% payload.templateData.properties.items.length].name}\"",
                                    },
                                    await Animations.FadeIn("hint", 1020, 2500)
                                }
                            }
                        }
                    }
                }
            });

            return layout;

        }
        private static async Task<List<IComponent>> RenderItemDetailsLayout(Properties<MediaItem> properties, IAlexaSession session)
        {
            const string leftColumnSpacing = "36vw";
            var mediaItem = properties?.item;
            var type = mediaItem?.type;

            // ReSharper disable UseObjectOrCollectionInitializer
            var detailLayout = new List<IComponent>();
            //backdrop video and static images
            if (!string.IsNullOrEmpty(mediaItem?.videoBackdropSource))
            {
                detailLayout.Add(new Video()
                {
                    source = new List<Source>()
                    {
                        new Source()
                        {
                            url = "${data.url}${data.item.videoBackdropSource}",
                            repeatCount = 0,
                        }
                    },
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    autoplay = true,
                    audioTrack = "none",
                    id = "${data.item.id}",
                    onEnd = new List<ICommand>()
                    {
                        new SetValue()
                        {
                            componentId = "backdropOverlay",
                            property = "source",
                            value = "${data.url}${data.item.backdropImageSource}"
                        },
                        new SetValue()
                        {
                            componentId = "backdropOverlay",
                            property = "opacity",
                            value = 1
                        },
                        new SetValue()
                        {
                            componentId = "backdropOverlay",
                            property = "overlayColor",
                            value = "rgba(0,0,0,0.55)"
                        }
                    }
                });
            }

            detailLayout.Add(new Image()
            {
                scale = "best-fill",
                width = "100vw",
                height = "100vh",
                position = "absolute",
                // If video backdrop exists in the dataSource, handle an opaque black value for the overlay image (blank.png), and allow the opacity property to handled the overlay transparency value.
                // If no video backdrop exists in the dataSource, handle the overlay for the static image that is shown and give it a 0.75 transparency black overlay.
                overlayColor = !string.IsNullOrEmpty(mediaItem?.videoBackdropSource) ? "rgba(0,0,0,1)" : "rgba(0,0,0,0.75)",
                // Show the back video overlay Image on top of the video
                // Show the static backdrop image
                source = !string.IsNullOrEmpty(mediaItem?.videoBackdropSource) ? "${data.url}${data.item.videoOverlaySource}" : "${data.url}${data.item.backdropImageSource}",
                // Video backdrop exists in the dataSource, which means this image will be an overlay, is black and should be given a 0.65 transparency
                // No video backdrop exists in the dataSource, so the static background image must be given no opacity (value: 1), and the "overlay" property will handle ... ... the overlay.
                opacity = !string.IsNullOrEmpty(mediaItem?.videoBackdropSource) ? 0.65 : 1,
                id = "backdropOverlay"
            });

            if (session.paging.canGoBack)
            {
                detailLayout.Add(new AlexaIconButton()
                {
                    vectorSource = MaterialVectorIcons.Left,
                    buttonSize = "15vh",
                    position = "absolute",
                    left = "2vw",
                    color = "white",
                    top = "-1vw",
                    id = "goBack",
                    primaryAction = new Parallel()
                    {
                        commands = new List<ICommand>()
                        {
                           new Command() { type = nameof(Animations.ScaleInOutOnPress) },
                           new SendEvent() { arguments = new List<object>() { "goBack" } }
                        }
                    }
                });
            }
            //Logo
            detailLayout.Add(new Image()
            {
                id = "logo",
                source = "${data.url}${data.item.logoImageSource}",
                width = "12vw",
                position = "absolute",
                left = "85vw",
            });
            //Name
            detailLayout.Add(new Text()
            {
                text = "${data.item.name}",
                style = "textStylePrimary",
                left = leftColumnSpacing,
                fontWeight = "100",
                top = "15vh",
                id = "baseItemName",
                opacity = 1,

            });
            //Genres
            detailLayout.Add(new Text()
            {
                text = "${data.item.genres}",
                left = leftColumnSpacing,
                style = "textStyleBody",
                top = "15vh",
                width = "40vw",
                height = "22dp",
                fontSize = "18dp",
                opacity = 1,
                id = "genre",

            });
            //Rating - Runtime - End time
            //Runtime span
            detailLayout.Add(new Text()
            {
                text = "${data.item.premiereDate} | ${data.item.officialRating} | ${data.item.runtimeMinutes} | ${data.item.endTime}",
                left = leftColumnSpacing,
                style = "textStyleBody",
                top = "17vh",
                width = "40vw",
                height = "22dp",
                fontSize = "18dp",
                opacity = 1,
                id = "rating",

            });
            //TagLines
            detailLayout.Add(new Text()
            {
                text = "${data.item.tagLine}",
                style = "textStyleBody",
                left = leftColumnSpacing,
                top = "18vh",
                height = "10dp",
                width = "40vw",
                fontSize = "22dp",
                id = "tag",
                display = !string.IsNullOrEmpty(mediaItem?.tagLine) ? "normal" : "none",
                opacity = 1,
            });
            //Watched check-mark
            detailLayout.Add(new VectorGraphic()
            {
                source = "CheckMark",
                left = "87vw",
                position = "absolute",
                top = "30vh"
            });
            //Overview
            detailLayout.Add(new TouchWrapper()
            {
                top = string.Equals(type, "Movie") ? "24vh" : "20vh",
                left = leftColumnSpacing,
                maxHeight = "20vh",
                opacity = 1,
                id = "overview_${data.item.id}",
                speech = "${data.readOverview.url}",
                onPress = new SpeakItem() { componentId = "overview_${data.item.id}" },
                item = new Container()
                {
                    items = new List<IComponent>()
                    {
                        new Container()
                        {
                            direction = "row",
                            items = new List<IComponent>()
                            {
                                new Text()
                                {
                                    fontSize = "22dp",
                                    text     = "<b>Overview</b>",
                                    style    = "textStyleBody",
                                    width    = "35vw",
                                    id       = "overviewHeader",
                                    opacity  = 1,

                                },
                                new VectorGraphic()
                                {
                                    source  = "Audio",
                                    right   = "20vw",
                                    opacity = 1,
                                    id      = "audioIcon",
                                    top     = "5px",

                                }
                            }
                        },
                        new Text()
                        {
                            text      = "${data.item.overview}",
                            style     = "textStyleBody",
                            maxHeight = "20vh",
                            id        = "overview",
                            width     = "55vw",
                            fontSize  = "20dp",
                            opacity   = 1,

                        }
                    }
                },

            });
            //Recommendations
            detailLayout.Add(new Container()
            {
                when = "${viewport.shape == 'rectangle' && viewport.mode == 'hub' && viewport.width > 960 && payload.templateData.properties.similarItems.length > 0}",
                width = "50vw",
                height = "250px",
                top = "29vh",
                left = "36vw",
                opacity = 1,
                items = new List<IComponent>()
                {
                    new Text()
                    {
                        text     = "Recommendations",
                        style    = "textStylePrimary",
                        fontSize = "20"
                    },
                    new Sequence()
                    {
                        width = "50vw",
                        height = "250px",
                        scrollDirection = "horizontal",
                        data = "${payload.templateData.properties.similarItems}",
                        items = new List<IComponent>()
                        {
                            new TouchWrapper()
                            {
                                width = "245px",
                                id = "${data.id}",
                                item = new Image()
                                {
                                    height = "140px",
                                    width = "225px",
                                    source = "${payload.templateData.properties.url}${data.thumbImageSource}",
                                },
                                onPress = new Parallel()
                                {
                                    commands = new List<ICommand>()
                                    {
                                        new Command() { type = nameof(Animations.ScaleInOutOnPress) },
                                        new SendEvent() { arguments = GetSequenceItemsOnPressArguments(type, session) }
                                    }
                                }
                            }
                        }
                    }
                },

            });
            //Series - Season Count
            if (string.Equals(type, "Series"))
            {
                detailLayout.Add(new Container()
                {
                    position = "absolute",
                    left = "38vw",
                    top = "78vh",
                    direction = "row",
                    items = new List<IComponent>()
                    {
                        new VectorGraphic()
                        {
                            id     = "SeasonCarouselArrayIcon",
                            source = "ArrayIcon"
                        },
                        new VectorGraphic()
                        {
                            id       = "SeasonCarouselIcon",
                            source   = "Carousel",
                            position = "absolute",
                            opacity  = 1
                        },
                        new Text()
                        {
                            style    = "textStyleBody",
                            fontSize = "23dp",
                            left     = "24dp",
                            width    = "25vw",
                            text     = "${data.item.TotalRecordCount} Available Season(s)"
                        }
                    }
                });
            }
            //Primary Image
            detailLayout.Add(new Container()
            {
                id = "primaryButton",
                position = "absolute",
                width = "38%",
                height = "75vh",
                top = "15vh",
                opacity = 1,
                items = new List<IComponent>()
                {
                    new Image()
                    {
                        source = "${data.url}${data.item.primaryImageSource}",
                        scale  = "best-fit",
                        height = "63vh",
                        width  = "100%",
                        id     = "primary"
                    },
                    await RenderComponent_PlayButton(mediaItem, session)
                }
            });

            detailLayout.Add(new AlexaFooter()
            {
                hintText = type == "Series" ? "Try \"Alexa, show season one...\"" : "Try \"Alexa, play that...\"",
                position = "absolute",
                bottom = "1vh"
            });

            var chapterLayout = new List<IComponent>();
            chapterLayout.Add(new Container()
            {
                height = "100vh",
                width = "100vw",
                alignItems = "center",
                items = new List<IComponent>()
                {
                    new Text()
                    {
                        text = "Chapters",
                        paddingTop = "5vh"
                    },
                    new Sequence()
                    {
                        top = "10vh",
                        height                 = "80vh",
                        width                  = "95vw",
                        left                   = "5vw",
                        scrollDirection        = "horizontal",
                        data                   = "${payload.templateData.properties.item.chapterData}",
                        items = new List<IComponent>()
                        {
                            new TouchWrapper()
                            {
                                width = "545px",
                                height = "550px",
                                id = "${payload.templateData.properties.item.id}",
                                items = new List<IComponent>()
                                {
                                    new Container()
                                    {
                                        width = "350px",
                                        bind = new List<DataBind>()
                                        {
                                            new DataBind()
                                            {
                                                name = "internalIndex",
                                                value = "${index}"
                                            }
                                        },
                                        direction = "column",
                                        items = new List<IComponent>()
                                        {
                                            new Image()
                                            {
                                                scale = "best-fill",
                                                height = "240px",
                                                width = "525px",
                                                source = "${payload.templateData.properties.url}/Items/${payload.templateData.properties.item.id}/Images/Chapter/${internalIndex}",
                                            },
                                            new Text()
                                            {
                                                text = "Chapter ${internalIndex+1}",
                                                style = "textStyleBody",
                                                fontSize = "25dp"
                                            },
                                            new Text()
                                            {
                                                text = "${data.Name}",
                                                style = "textStyleBody",
                                                fontSize = "25dp"
                                            },
                                            new Text()
                                            {
                                                bind = new List<DataBind>()
                                                {
                                                    new DataBind()
                                                    {
                                                        name = "T",
                                                        value = "${payload.templateData.properties.item.chapterData[internalIndex].StartPositionTicks /10000}"
                                                    }
                                                },
                                                text = "${Time.format('mm:ss',T)}",
                                                style = "textStyleBody",
                                                fontSize = "25dp"
                                            }
                                        }
                                    }

                                },
                                onPress = new Parallel()
                                {
                                    commands = new List<ICommand>()
                                    {
                                        new Command() { type = nameof(Animations.ScaleInOutOnPress) },
                                        new SendEvent() { arguments = new List<object>(){ nameof(UserEventPlaybackStart), session.room is null ? "" : session.room.Name, "${data.StartPositionTicks}"} }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return await Task.FromResult(new List<IComponent>()
            {
                new Pager()
                {
                    height        = "100vh",
                    width         = "100vw",
                    initialPage   = 0,
                    items = new List<IComponent>()
                    {
                        new Container()
                        {
                            bind = new List<DataBind>()
                            {
                                new DataBind()
                                {
                                    name = "data",
                                    value = "${payload.templateData.properties}"
                                }
                            },
                            width  = "100vw",
                            height = "100vh",
                            items  = detailLayout,
                        },
                        new Frame()
                        {
                            width  = "100vw",
                            height = "100vh",
                            backgroundColor = "rgba(95,95,95)",
                            items  = chapterLayout,
                        }
                    }
                }

            });
        }
        private static async Task<List<IComponent>> RenderRoomSelectionLayout(IAlexaSession session)
        {
            var layout = new List<IComponent>
            {
                new Video()
                {
                    source = new List<Source>()
                    {
                        new Source()
                        {
                            url = "${data.url}${data.item.videoBackdropSource}", repeatCount = 0,
                        }
                    },
                    scale      = "best-fill",
                    width      = "100vw",
                    height     = "100vh",
                    position   = "absolute",
                    autoplay   = true,
                    audioTrack = "none",
                    id         = "${data.item.id}",
                    onEnd = new List<ICommand>()
                    {
                        new SetValue()
                        {
                            componentId = "backdropOverlay",
                            property    = "source",
                            value       = "${data.url}${data.item.backdropImageSource}"
                        },
                        new SetValue() {componentId = "backdropOverlay", property = "opacity", value = 1},
                        new SetValue()
                        {
                            componentId = "backdropOverlay",
                            property    = "overlayColor",
                            value       = "rgba(0,0,0,0.55)"
                        }
                    }
                },
                new Image()
                {
                    overlayColor = "rgba(0,0,0,1)",
                    scale        = "best-fill",
                    width        = "100vw",
                    height       = "100vh",
                    position     = "absolute",
                    source       = "${data.url}${data.item.videoOverlaySource}",
                    opacity      = 0.65,
                    id           = "backdropOverlay"
                },
                new AlexaHeader()
                {
                    headerBackButton = true,
                    headerTitle      = "${data.item.name}",
                    headerSubtitle   = $"{session.User.Name} Play On...",
                    headerDivider    = true
                },
                new Image()
                {
                    position = "absolute",
                    source   = "${data.url}${data.item.logoImageSource}",
                    width    = "25vw",
                    height   = "10vh",
                    right    = "5vw",
                    bottom   = "5vh"
                }
            };
            var roomButtons = RenderComponent_RoomButtonLayoutContainer();
            roomButtons.ForEach(b => layout.Add(b));

            return await Task.FromResult(new List<IComponent>()
            {
                new Container()
                {
                    width = "100vw",
                    bind = new List<DataBind>()
                    {
                        new DataBind()
                        {
                            name  = "data",
                            value = "${payload.templateData.properties}"
                        }
                    },
                    items = layout
                }
            });
        }
        private static async Task<List<IComponent>> RenderGenericViewLayout()
        {
            return await Task.FromResult(new List<IComponent>
            {
                new Container()
                {
                    width  = "100vw",
                    height = "100vh",
                    items  = new List<IComponent>()
                    {
                        new Video()
                        {
                            source = new List<Source>()
                            {
                                new Source()
                                {
                                    url = "${payload.templateData.properties.url}${payload.templateData.properties.videoUrl}",
                                    repeatCount = 1,
                                }
                            },
                            scale      = "best-fill",
                            width      = "100vw",
                            height     = "100vh",
                            position   = "absolute",
                            autoplay   = true,
                            audioTrack = "none"
                        },
                        new Image()
                        {
                            overlayColor = "rgba(0,0,0,1)",
                            scale        = "best-fill",
                            width        = "100vw",
                            height       = "100vh",
                            position     = "absolute",
                            source       = "${payload.templateData.properties.url}/EmptyPng?quality=90",
                            opacity      = 0.35
                        },
                        new AlexaHeadline()
                        {
                            backgroundColor = "rgba(0,0,0,0.1)", primaryText = "${payload.templateData.properties.text}"
                        },
                        new AlexaFooter() {hintText = "Alexa, open help...", position = "absolute", bottom = "1vh"}
                    }
                }
            });

        }
        private static async Task<List<IComponent>> RenderHelpViewLayout()
        {
            return await Task.FromResult(new List<IComponent>()
            {
                new Container()
                {
                    width  = "100vw",
                    height = "100vh",
                    id     = "helpPageContainer",
                    items  = new List<IComponent>()
                    {
                        new Frame()
                        {
                            id              = "logoFrame",
                            width           = "100vw",
                            height          ="100vh",
                            backgroundColor = "white",
                            items = new List<IComponent>()
                            {
                                new Container()
                                {
                                    direction      = "row",
                                    alignItems     = "center",
                                    justifyContent = "center",
                                    width          = "100vw",
                                    height         = "100vh",
                                    items = new List<IComponent>()
                                    {
                                        new VectorGraphic()
                                        {
                                            bind = new List<DataBind>()
                                            {
                                                new DataBind()
                                                {
                                                    name = "strokeDashOffset",
                                                    value = 0
                                                },
                                                new DataBind()
                                                {
                                                    name = "fill",
                                                    value = "none"
                                                },
                                                new DataBind()
                                                {
                                                    name = "stroke",
                                                    value = "#00b0e6"
                                                }
                                            },
                                            source           = "AlexaLarge",
                                            id               = "logoAlexa",
                                            strokeDashOffset = "${strokeDashOffset}",
                                            fill             = "${fill}",
                                            stroke           = "${stroke}",
                                            width            = "50vw",
                                            height           = "50vh",
                                            opacity          = 1,
                                            onMount = new List<ICommand>()
                                            {
                                                //Draw In, Fadeout
                                                new Sequential()
                                                {
                                                    commands =  new List<ICommand>()
                                                    {
                                                        //Draw
                                                        new Sequential()
                                                        {
                                                            repeatCount = 65,
                                                            commands = new List<ICommand>()
                                                            {
                                                                new SetValue()
                                                                {
                                                                    property = "strokeDashOffset",
                                                                    value    = "${(strokeDashOffset + 1)}",
                                                                    delay    = 20
                                                                },
                                                                new SetValue()
                                                                {
                                                                    when     = "${strokeDashOffset > 64}",
                                                                    property = "fill",
                                                                    value    = "#00b0e6",
                                                                    delay    = 20
                                                                },
                                                                new SetValue()
                                                                {
                                                                    when     = "${strokeDashOffset > 64}",
                                                                    property = "stroke",
                                                                    value    = "none",
                                                                    delay    = 20
                                                                }
                                                            }
                                                        },
                                                        //Fade
                                                        await Animations.FadeOut("logoAlexa", 1000, 1000)
                                                    }
                                                }

                                            }
                                        },
                                        new VectorGraphic()
                                        {
                                            bind = new List<DataBind>()
                                            {
                                                new DataBind()
                                                {
                                                    name = "strokeDashOffset",
                                                    value = 0
                                                },
                                                new DataBind()
                                                {
                                                    name = "fill",
                                                    value = "none"
                                                },
                                                new DataBind()
                                                {
                                                    name = "stroke",
                                                    value = "rgba(81,201,39)"
                                                }
                                            },
                                            source           = "EmbyLarge",
                                            strokeDashOffset = "${strokeDashOffset}",
                                            fill             = "${fill}",
                                            stroke           = "${stroke}",
                                            position         = "absolute",
                                            width            = "30vw",
                                            height           = "50vh",
                                            id               = "logoEmby",
                                            opacity          = 1,
                                            onMount = new List<ICommand>()
                                            {
                                                //Draw In, Fadeout
                                                new Sequential()
                                                {
                                                    delay = 3000,
                                                    commands = new List<ICommand>()
                                                    {
                                                        //Draw
                                                        new Sequential()
                                                        {
                                                            repeatCount = 65,
                                                            commands = new List<ICommand>()
                                                            {
                                                                new SetValue()
                                                                {
                                                                    property = "strokeDashOffset",
                                                                    value    = "${(strokeDashOffset + 1)}",
                                                                    delay    = 20
                                                                },
                                                                new SetValue()
                                                                {
                                                                    when     = "${strokeDashOffset > 64}",
                                                                    property = "fill",
                                                                    value    = "rgba(81,201,39)",
                                                                    delay    = 20
                                                                },
                                                                new SetValue()
                                                                {
                                                                    when     = "${strokeDashOffset > 64}",
                                                                    property = "stroke",
                                                                    value    = "none",
                                                                    delay    = 20
                                                                }
                                                            }
                                                        },
                                                        await Animations.FadeOut("logoEmby", 1000, 1000)
                                                    }
                                                }

                                            }
                                        }
                                    },
                                    onMount = new List<ICommand>()
                                    {
                                        new Sequential()
                                        {
                                            commands = new List<ICommand>()
                                            {
                                                new SetValue()
                                                {
                                                    componentId = "logoFrame",
                                                    property = "display",
                                                    value = "none",
                                                    delay = 6000
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                        },
                        new Pager()
                        {
                            height        = "100vh",
                            width         = "100vw",
                            initialPage   = 0,
                            navigation    = "forward-only",
                            data          = "${payload.templateData.properties.values}",
                            items = new List<IComponent>()
                            {
                                new Container()
                                {
                                    justifyContent = "center",
                                    alignItems     = "center",
                                    direction      = "column",
                                    bind = new List<DataBind>()
                                    {
                                        new DataBind()
                                        {
                                            name = "internalIndex",
                                            value ="${index}"
                                        }
                                    },
                                    items = new List<IComponent>()
                                    {
                                        new VectorGraphic()
                                        {
                                            source = "EmbySmall",
                                        },
                                        new VectorGraphic()
                                        {
                                            source = "Line",
                                        },
                                        new Text()
                                        {
                                            text              = "${data.value}",
                                            id                = "page_${internalIndex}",
                                            textAlign         = "center",
                                            textAlignVertical = "center",
                                            paddingRight      = "20dp",
                                            paddingLeft       = "20dp",
                                            speech            = "${payload.templateData.properties.values[internalIndex].helpPhrase}"
                                        },
                                        new Text()
                                        {
                                            text              = "${internalIndex} | ${payload.templateData.properties.values.length-1}",
                                            textAlign         = "center",
                                            textAlignVertical = "center",
                                            paddingRight      = "20dp",
                                            paddingLeft       = "20dp"
                                        }
                                    }
                                }
                            },
                            id = "HelpPager",
                            onPageChanged = new List<ICommand>()
                            {
                                new Sequential()
                                {
                                    commands = new List<ICommand>()
                                    {
                                        new SpeakItem()
                                        {
                                            componentId = "page_${event.source.page}"
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            });
        }

        //Create template components 
        private static async Task<Frame> RenderComponent_PlayButton(MediaItem item, IAlexaSession session)
        {
            List<object> args = null;
            switch (item.type)
            {
                case "Episode":
                case "Movie":
                    args = new List<object>() { nameof(UserEventPlaybackStart), session.room != null ? session.room.Name : "" };
                    break;
                default:
                    args = new List<object>() { nameof(UserEventShowItemListSequenceTemplate) };
                    break;
            }

            return await Task.FromResult(new Frame()
            {
                position = "absolute",
                top = "29vh",
                right = "40%",
                borderWidth = "3px",
                borderColor = "white",
                borderRadius = "75px",
                backgroundColor = "rgba(0,0,0,0.35)",
                items = new List<IComponent>()
                {
                    new AlexaIconButton()
                    {
                        vectorSource = item.type == "Series" ?  MaterialVectorIcons.ListIcon : MaterialVectorIcons.PlayOutlineIcon,
                        buttonSize = "13vh",
                        id = item.id.ToString(),

                        primaryAction = new Sequential()
                        {
                            commands = new List<ICommand>()
                            {
                                new Parallel()
                                {
                                    commands = new List<ICommand>()
                                    {
                                        new Command()   { type = nameof(Animations.ScaleInOutOnPress) },
                                    }
                                },
                                new SendEvent() { arguments = args },


                            }
                        }
                    }
                }
            });
        }
        private static async Task<Container> RenderComponent_SequencePrimaryImageContainer(string type)
        {
            switch (type)
            {
                case "Episode":
                    return await Task.FromResult(new Container()
                    {
                        height = "70vh",
                        width = "30vw",
                        items = new List<IComponent>()
                        {
                            new Image()
                            {
                                source       = "${payload.templateData.properties.url}${data.primaryImageSource}",
                                width        = "30vw",
                                height       = "52vh",
                                paddingRight = "12px",
                                paddingBottom = "15vh"
                            },
                            new Text()
                            {
                                text         = "${data.name}",
                                style        = "textStyleBody",
                                top          = "-15vh",
                                fontSize     = "20dp"
                            },
                            new Text()
                            {
                                text         = "${data.index}",
                                style        = "textStyleBody",
                                top          = "-15vh",
                                fontSize     = "20dp"
                            },
                            new Text()
                            {
                                text         = "${data.premiereDate}",
                                style        = "textStyleBody",
                                top          = "-15vh",
                                fontSize     = "15dp"
                            }
                        }
                    });
                default:
                    return await Task.FromResult(new Container()
                    {
                        items = new List<IComponent>()
                        {
                            new Image()
                            {
                                source       = "${payload.templateData.properties.url}${data.primaryImageSource}",
                                width        = "30vw",
                                height       = "62vh",
                                paddingRight = "12px",
                            }
                        }
                    });
            }

        }
        private static List<IComponent> RenderComponent_RoomButtonLayoutContainer()
        {
            var config = Plugin.Instance.Configuration;
            var roomButtons = new List<IComponent>();

            if (config.Rooms is null) return roomButtons;

            System.Threading.Tasks.Parallel.ForEach(config.Rooms, room =>
            {
                var disabled = true;

                foreach (var session in ServerQuery.Instance.GetCurrentSessions())
                {
                    if (session.DeviceName == room.DeviceName) disabled = false;
                    if (session.Client == room.DeviceName) disabled = false;
                }

                roomButtons.Add(new Container()
                {
                    direction = "row",
                    left = "15vw",
                    top = "10vh",
                    items = new List<IComponent>()
                    {
                        new AlexaIconButton()
                        {
                            id            = "${data.item.id}",
                            buttonSize    = "72dp",
                            vectorSource  = MaterialVectorIcons.CastIcon,
                            disabled      = disabled,
                            primaryAction = new Sequential()
                            {
                                commands = new List<ICommand>()
                                {
                                    new SendEvent() { arguments = new List<object>() { nameof(UserEventPlaybackStart), room.Name } },
                                }
                            }

                        },
                        new Text()
                        {
                            text        = $"{room.Name} " + (disabled ? " (unavailable)" : string.Empty),
                            paddingTop  = "12dp",
                            paddingLeft = "5vw"
                        },
                    }
                });
            });

            return roomButtons;
        }

        private static List<object> GetSequenceItemsOnPressArguments(string type, IAlexaSession session)
        {
            var room = session.room != null ? session.room.Name : "";
            //TODO: Do we want to show episodes Detail page from sequence, or just play the item?
            switch (type)
            {
                case "Movie":
                case "Series":
                case "Trailer": return new List<object>() { nameof(UserEventShowBaseItemDetailsTemplate) };
                case "Episode": return new List<object>() { nameof(UserEventPlaybackStart), room };
                default: return new List<object>() { nameof(UserEventShowItemListSequenceTemplate) };
            }
        }
    }
}
