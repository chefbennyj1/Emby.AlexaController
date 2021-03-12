using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.Components;
using AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press;
using AlexaController.Alexa.Presentation.APL.VectorGraphics;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using AlexaController.DataSourceProperties;
using AlexaController.Session;
using Parallel = AlexaController.Alexa.Presentation.APL.Commands.Parallel;
using Source   = AlexaController.Alexa.Presentation.APL.Components.Source;
using Text     = AlexaController.Alexa.Presentation.APL.Components.Text;
using Video    = AlexaController.Alexa.Presentation.APL.Components.Video;

// ReSharper disable twice InconsistentNaming

/*
 * Echo Display Devices use the **LAN address** for Images when using the skill on the same network as emby server (ex. 192.168.X.XXX:8096)
 * We do not need to serve media requests over https like the documentation implies.
 * https://developer.amazon.com/en-US/docs/alexa/alexa-presentation-language/apl-image.html
 */

namespace AlexaController
{
    public class AplRenderDocumentDirectiveManager
    {
        public static AplRenderDocumentDirectiveManager Instance { get; private set; }

        public AplRenderDocumentDirectiveManager()
        {
            Instance = this;
        }

        private readonly List<Import> Imports = new List<Import>()
        {
            new Import()
            {
                name                          = "alexa-layouts",
                version                       = "1.2.0"
            },
            new Import()
            {
                name                          = "alexa-viewport-profiles",
                version                       = "1.1.0"
            }
        };

        private readonly List<Resource> Resources = new List<Resource>()
        {
            new Resource()
            {
                description = "Stock color for the light theme",
                colors      = new Colors()
                {
                    colorTextPrimary = "#151920"
                }
            },
            new Resource()
            {
                description = "Stock color for the dark theme",
                when        = "${viewport.theme == 'dark'}",
                colors      = new Colors()
                {
                    colorTextPrimary = "#f0f1ef"
                }
            },
            new Resource()
            {
                description = "Standard font sizes",
                dimensions  = new Dimensions()
                {
                    textSizeBody          = 48,
                    textSizePrimary       = 27,
                    textSizeSecondary     = 23,
                    textSizeSecondaryHint = 25
                }
            },
            new Resource()
            {
                description = "Common spacing values",
                dimensions  = new Dimensions()
                {
                    spacingThin       = 6,
                    spacingSmall      = 12,
                    spacingMedium     = 24,
                    spacingLarge      = 48,
                    spacingExtraLarge = 72
                }
            },
            new Resource()
            {
                description = "Common margins and padding",
                dimensions  = new Dimensions()
                {
                    marginTop    = 40,
                    marginLeft   = 60,
                    marginRight  = 60,
                    marginBottom = 40
                }
            }
        };

        public async Task<IDirective> GetRenderDocumentDirectiveAsync<T>(IDataSource dataSource, IAlexaSession session) where T : class
        {
            var properties = (Properties<T>)dataSource.properties;
            List<IComponent> layout = null;
            switch (properties.documentType)
            {
                case RenderDocumentType.GENERIC_VIEW                : layout = await RenderGenericViewLayout(dataSource); 
                    break;
                case RenderDocumentType.ITEM_DETAILS_TEMPLATE       : layout = await RenderItemDetailsLayout(dataSource, session); 
                    break;
                case RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE : layout = await RenderItemListSequenceLayout(dataSource, session);
                    break;
                case RenderDocumentType.ROOM_SELECTION_TEMPLATE     : layout = await RenderRoomSelectionLayout(dataSource, session);
                    break;
                case RenderDocumentType.HELP                        : layout =  await RenderHelpView(dataSource);
                    break;
                default                                             : return null;
            }

            var graphicsDictionary = new Dictionary<string, AlexaVectorGraphic>
            {
                {
                    "CheckMark", new AlexaVectorGraphic()
                    {
                        height         = 35,
                        width          = 35,
                        viewportHeight = 25,
                        viewportWidth  = 25,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    = MaterialVectorIcons.CheckMark,
                                stroke      = "none",
                                strokeWidth = 1,
                                fill        = "rgba(255,0,0,1)" 
                            }
                        }
                    }
                },
                {
                    "Audio", new AlexaVectorGraphic()
                    {
                        height         = 25,
                        width          = 25,
                        viewportHeight = 28,
                        viewportWidth  = 28,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    = MaterialVectorIcons.Audio,
                                stroke      = "none",
                                strokeWidth = 1,
                                fill        = "white"
                            }
                        }
                    }
                },
                {
                    "Carousel", new AlexaVectorGraphic()
                    {
                        height         = 35,
                        width          = 35,
                        viewportHeight = 25,
                        viewportWidth  = 25,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    =  MaterialVectorIcons.Carousel,
                                stroke      = "none",
                                strokeWidth = 1,
                                fill        = "rgba(255,250,0,1)"
                            }
                        }
                    }
                },
                {
                    "ArrayIcon", new AlexaVectorGraphic()
                    {
                        height         = 35,
                        width          = 35,
                        viewportHeight = 25,
                        viewportWidth  = 25,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    =  MaterialVectorIcons.ArrayIcon,
                                stroke      = "none",
                                strokeWidth = 1,
                                fill        = "rgba(255,250,0,1)"
                            }
                        }
                    }
                },
                  {
                    "AlexaLarge", new AlexaVectorGraphic()
                    {
                        parameters = new List<string>()
                        {
                            "strokeDashOffset",
                            "fill",
                            "stroke"
                        },
                        height         = 235,
                        width          = 235,
                        viewportHeight = 25,
                        viewportWidth  = 25,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    = MaterialVectorIcons.Alexa,
                                stroke      = "${stroke}",
                                strokeWidth = 0.5,
                                fill        = "${fill}",
                                strokeDashArray = new List<string>()
                                {
                                    "${strokeDashOffset}",
                                    "100"
                                },
                                strokeDashOffset = 0,//"rgba(20,200,255,1)",
                                filters = new List<VectorFilter>()
                                {
                                    new VectorFilter()
                                    {
                                        type = VectorFilterType.DropShadow,
                                        color = "rgba(0,0,0,0.375)",
                                        horizontalOffset = 0.005,
                                        verticalOffset = 0.005,
                                        radius = 1
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "EmbyLarge", new AlexaVectorGraphic()
                    {
                        parameters = new List<string>()
                        {
                            "strokeDashOffset",
                            "fill",
                            "stroke"
                        },
                        height         = 240,
                        width          = 240,
                        viewportHeight = 25,
                        viewportWidth  = 25,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    = MaterialVectorIcons.EmbyIcon,
                                stroke      = "${stroke}",
                                strokeWidth = 0.5,
                                fill        = "${fill}",
                                strokeDashArray = new List<string>()
                                {
                                    "${strokeDashOffset}",
                                    "100"
                                },
                                strokeDashOffset = 0, //"rgba(81,201,39)",
                                filters = new List<VectorFilter>()
                                {
                                    new VectorFilter()
                                    {
                                        type = VectorFilterType.DropShadow,
                                        color = "rgba(0,0,0,0.375)",
                                        horizontalOffset = 0.005,
                                        verticalOffset = 0.005,
                                        radius = 1
                                    }
                                }
                            }
                        }
                    }
                },
                {
                    "EmbySmall", new AlexaVectorGraphic()
                    {
                        height         = 75,
                        width          = 75,
                        viewportHeight = 25,
                        viewportWidth  = 25,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    = MaterialVectorIcons.EmbyIcon,
                                stroke      = "none",
                                strokeWidth = 0,
                                fill        = "white"
                            }
                        }
                    }
                },
                {
                    "Line", new AlexaVectorGraphic()
                    {
                        height         = 55,
                        width          = 500,
                        viewportWidth  = 50,
                        viewportHeight = 50,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    = "M0 0 l1120 0",
                                stroke      = "rgba(255,255,255)",
                                strokeWidth = 1
                            }
                        }
                    }
                }
            };

            return await Task.FromResult(new Directive()
            {
                type = Directive.AplRenderDocument,
                token = properties.documentType.ToString(),
                document = new Document()
                {
                    theme = "${payload.templateData.properties.theme}",
                    import = Imports,
                    resources = Resources,
                    graphics = graphicsDictionary,
                    commands = new Dictionary<string, ICommand>()
                    {
                        { nameof(AnimationFactory.ScaleInOutOnPress), await AnimationFactory.ScaleInOutOnPress() },
                        { nameof(AnimationFactory.FadeIn), await AnimationFactory.FadeIn() } 
                    },
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = layout
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { { "templateData", dataSource } },
                sources = new Dictionary<string, IDocument>()
                {
                    { "buttonPressEffectApla", new Alexa.Presentation.APLA.Document()
                        {
                            mainTemplate = new MainTemplate()
                            {
                                parameters = new List<string>() { "payload" },
                                item = new Audio()  { source = "soundbank://soundlibrary/camera/camera_15" }
                            }
                        }
                    }
                }
            });
        }
        
        private async Task<List<IComponent>> RenderItemListSequenceLayout(IDataSource dataSource, IAlexaSession session)
        {
            var layout           = new List<IComponent>();
            var properties       = (Properties<MediaItem>) dataSource.properties;
            var baseItems        = properties.items;
            var type             = baseItems[0].type;
            
            layout.Add(new Container()
            {
                id = "primary",
                width = "100vw",
                height = "100vh",
                items = new List<IComponent>()
                {
                    new Image()
                    {
                        height = "100%",
                        overlayColor = "rgba(0,0,0,0.65)",
                        width = "100%",
                        scale = "best-fill",
                        position = "absolute",
                        source = "${payload.templateData.properties.url}${payload.templateData.properties.item.backdropImageSource}",
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
                                speech = "${payload.templateData.properties.buttonPressEffect.url}",
                                onPress    = new Parallel()
                                {
                                   commands = new List<ICommand>()
                                   {
                                       new SpeakItem() { componentId = "${data.id}" },
                                       new Command() { type = nameof(AnimationFactory.ScaleInOutOnPress) },
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
                                    new AnimateItem()
                                    {
                                        componentId = "hint",
                                        duration    = 1020,
                                        easing      = "ease-in",
                                        value       = new List<IValue>()
                                        {
                                            new OpacityValue() { @from = 1, to = 0 }
                                        },
                                        delay = 5000
                                    },
                                    new SetValue()
                                    {
                                        componentId = "hint",
                                        property    = "hintText",
                                        value       =  "Try \"Alexa, Show The ${payload.templateData.properties.items[0].type}: ${payload.templateData.properties.items[Time.seconds(localTime/payload.templateData.properties.items.length) % payload.templateData.properties.items.length].name}\"",
                                    },
                                    new AnimateItem()
                                    {
                                        componentId = "hint",
                                        duration    = 1020,
                                        easing      = "ease-out",
                                        value       = new List<IValue>()
                                        {
                                            new OpacityValue() { @from = 0, to = 1 }
                                        },
                                        delay = 2500
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return layout;

        }

        private async Task<List<IComponent>> RenderItemDetailsLayout(IDataSource dataSource, IAlexaSession session)
        {
            const string leftColumnSpacing = "36vw";
            var properties = (Properties<MediaItem>) dataSource.properties;
            var mediaItem  = properties.item;
            var type       = mediaItem.type;
           
            // ReSharper disable UseObjectOrCollectionInitializer
            var layout   = new List<IComponent>();
            //backdrop video and static images
            layout.Add(new Video()
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
                        property    = "source",
                        value       = "${data.url}${data.item.backdropImageSource}"
                    },
                    new SetValue()
                    {
                        componentId = "backdropOverlay",
                        property    = "opacity",
                        value       = 1
                    },
                    new SetValue()
                    {
                        componentId = "backdropOverlay",
                        property    = "overlayColor",
                        value       = "rgba(0,0,0,0.55)"
                    }
                }
            });
            layout.Add(new Image()
            {
                overlayColor = "rgba(0,0,0,1)",
                scale = "best-fill",
                width = "100vw",
                height = "100vh",
                position = "absolute",
                source = "${data.url}${data.item.videoOverlaySource}",
                opacity = 0.65,
                id = "backdropOverlay"
            });
            
            if (session.paging.canGoBack)
            {
                layout.Add(new AlexaIconButton()
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
                           new Command()
                           {
                               type = nameof(AnimationFactory.ScaleInOutOnPress)
                           },
                            new SendEvent() {arguments = new List<object>() {"goBack"}}
                        }
                    }
                });
            }

            layout.Add(new Image()
            {
                id = "logo",
                source = "${data.url}${data.item.logoImageSource}",
                width = "12vw",
                position = "absolute",
                left = "85vw",
            });
            //Name
            layout.Add(new Text()
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
            layout.Add(new Text()
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
            layout.Add(new Text()
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
            layout.Add(new Text()
            {
                text = "${data.item.tagLine}",
                style = "textStyleBody",
                left = leftColumnSpacing,
                top = "18vh",
                height = "10dp",
                width = "40vw",
                fontSize = "22dp",
                id = "tag",
                display = !string.IsNullOrEmpty(mediaItem.tagLine) ? "normal" : "none",
                opacity = 1,
            });
            //Watched check-mark
            layout.Add(new VectorGraphic()
            {
                source = "CheckMark",
                left = "87vw",
                position = "absolute",
                top = "30vh"
            });
            //Overview
            layout.Add(new TouchWrapper()
            {
                top       = string.Equals(type, "Movie") ? "24vh" : "20vh",
                left      = leftColumnSpacing,
                maxHeight = "20vh",
                opacity   = 1,
                id        = "overview_${data.item.id}",
                speech    = "${data.item.readOverview}",
                onPress   = new SpeakItem() { componentId = "overview_${data.item.id}"},
                item      = new Container()
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
            layout.Add(new Container()
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
                                        new Command() { type = nameof(AnimationFactory.ScaleInOutOnPress) },
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
                layout.Add(new Container()
                {
                    position = "absolute",
                    left = "38vw",
                    top = "78vh",
                    direction = "row",
                    items = new List<IComponent>()
                    {
                        new VectorGraphic()
                        {
                            id = "SeasonCarouselArrayIcon",
                            source = "ArrayIcon"
                        },
                        new VectorGraphic()
                        {
                            id = "SeasonCarouselIcon",
                            source = "Carousel",
                            position = "absolute",
                            opacity = 1
                        },
                        new Text()
                        {
                            style = "textStyleBody",
                            fontSize = "23dp",
                            left = "12dp",
                            text = "<b>" +
                                   ServerQuery.Instance.GetItemsResult(mediaItem.id, new []{"Season"}, session.User)
                                       .TotalRecordCount + "</b>"
                        },
                        new Text()
                        {
                            style = "textStyleBody",
                            fontSize = "23dp",
                            left = "24dp",
                            width = "25vw",
                            text = "Available Season(s)"
                        }
                    }
                });
            }
            //Primary Image
            layout.Add(new Container()
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
            layout.Add(new AlexaFooter()
            {
                hintText = type == "Series" ? "Try \"Alexa, show season one...\"" : "Try \"Alexa, play that...\"",
                position = "absolute",
                bottom = "1vh"
            });

            return await Task.FromResult(new List<IComponent>()
            {
                new Container()
                {
                    when   = "${viewport.shape == 'round'}",
                    width  = "100vw",
                    height = "100vh",
                    items  = new List<IComponent>()
                    {
                        new AlexaHeader()
                        {
                            headerTitle            = "",
                            headerAttributionImage = "",
                            headerBackButton       = true,
                            headerSubtitle         = session.User.Name,
                        },
                        new Image()
                        {
                            source = "${data.logoUrl}",
                            width  = "55%",
                            height = "25vh",
                            left   = "20%",
                            align  = "center"
                        },
                        new Text()
                        {
                            text     =  $"{(session.room != null ? session.room.Name : "")}",
                            left     = "10%",
                            width    = "85%",
                            height   = "5%",
                            fontSize = "16dp"
                        },
                    }
                },
                new Container()
                {
                    bind = new List<DataBind>()
                    {
                        new DataBind()
                        {
                            name = "data", 
                            value = "${payload.templateData.properties}"
                        },
                    },
                    width  = "100vw",
                    height = "100vh",
                    items  = layout,
                }
            });
        }

        private async Task<List<IComponent>> RenderRoomSelectionLayout(IDataSource dataSource, IAlexaSession session)
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
                        new SetValue() {componentId = "backdropOverlay", property = "opacity", value = 1},
                        new SetValue()
                        {
                            componentId = "backdropOverlay",
                            property = "overlayColor",
                            value = "rgba(0,0,0,0.55)"
                        }
                    }
                },
                new Image()
                {
                    overlayColor = "rgba(0,0,0,1)",
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    source = "${data.url}${data.item.videoOverlaySource}",
                    opacity = 0.65,
                    id = "backdropOverlay"
                },
                new AlexaHeader()
                {
                    headerBackButton = true,
                    headerTitle = "${data.item.name}",
                    headerSubtitle = $"{session.User.Name} Play On...",
                    headerDivider = true
                },
                new Image()
                {
                    position = "absolute",
                    source = "${data.url}${data.item.logoImageSource}",
                    width = "25vw",
                    height = "10vh",
                    right = "5vw",
                    bottom = "5vh"
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
        
        private async Task<List<IComponent>> RenderGenericViewLayout(IDataSource dataSource)
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

        private async Task<List<IComponent>> RenderHelpView(IDataSource dataSource)
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
                            id = "logoFrame",
                            width = "100vw",
                            height ="100vh",
                            backgroundColor = "white",
                            items = new List<IComponent>()
                            {
                                new Container()
                                {
                                    direction = "row",
                                    alignItems = "center",
                                    justifyContent = "center",
                                    width = "100vw",
                                    height = "100vh",
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
                                            source = "AlexaLarge",
                                            id = "logoAlexa",
                                            strokeDashOffset = "${strokeDashOffset}",
                                            fill = "${fill}",
                                            stroke = "${stroke}",
                                            width = "50vw",
                                            height = "50vh",
                                            opacity = 1,
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
                                                                    value = "${(strokeDashOffset + 1)}",
                                                                    delay = 20
                                                                },
                                                                new SetValue()
                                                                {
                                                                    when = "${strokeDashOffset > 64}",
                                                                    property = "fill",
                                                                    value = "#00b0e6",
                                                                    delay = 20
                                                                },
                                                                new SetValue()
                                                                {
                                                                    when = "${strokeDashOffset > 64}",
                                                                    property = "stroke",
                                                                    value = "none",
                                                                    delay = 20
                                                                }
                                                            }
                                                        },
                                                        //Fade
                                                        new AnimateItem()
                                                        {
                                                            easing = "ease-in",
                                                            componentId = "logoAlexa",
                                                            duration = 1000,
                                                            delay = 1000,
                                                            value = new List<IValue>()
                                                            {
                                                                new OpacityValue()
                                                                {
                                                                    @from = 1,
                                                                    to = 0
                                                                }
                                                            }
                                                        }
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
                                            source = "EmbyLarge",
                                            strokeDashOffset = "${strokeDashOffset}",
                                            fill = "${fill}",
                                            stroke = "${stroke}",
                                            position = "absolute",
                                            width = "30vw",
                                            height = "50vh",
                                            id = "logoEmby",
                                            opacity = 1,
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
                                                                    value = "${(strokeDashOffset + 1)}",
                                                                    delay = 20
                                                                },
                                                                new SetValue()
                                                                {
                                                                    when = "${strokeDashOffset > 64}",
                                                                    property = "fill",
                                                                    value = "rgba(81,201,39)",
                                                                    delay = 20
                                                                },
                                                                new SetValue()
                                                                {
                                                                    when = "${strokeDashOffset > 64}",
                                                                    property = "stroke",
                                                                    value = "none",
                                                                    delay = 20
                                                                }
                                                            }
                                                        },
                                                        //Fade
                                                        new AnimateItem()
                                                        {
                                                            easing = "ease-in",
                                                            componentId = "logoEmby",
                                                            duration = 1000,
                                                            delay = 1000,
                                                            value = new List<IValue>()
                                                            {
                                                                new OpacityValue()
                                                                {
                                                                    @from = 1,
                                                                    to = 0
                                                                }
                                                            }
                                                        }
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
                                    alignItems = "center",
                                    direction = "column",
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
                                            text = "${data.value}",
                                            id = "page_${internalIndex}",
                                            textAlign         = "center",
                                            textAlignVertical = "center",
                                            paddingRight      = "20dp",
                                            paddingLeft       = "20dp",
                                            speech = "${payload.templateData.properties.values[internalIndex].helpPhrase}"
                                        },
                                        new Text()
                                        {
                                            text = "${internalIndex} | ${payload.templateData.properties.values.length-1}",
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
                                        new Command()   { type = nameof(AnimationFactory.ScaleInOutOnPress) },
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

        private List<IComponent> RenderComponent_RoomButtonLayoutContainer()
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
                            id = "${data.item.id}",
                            buttonSize = "72dp",
                            vectorSource = MaterialVectorIcons.CastIcon,
                            disabled = disabled,
                            primaryAction = new Sequential()
                            {
                                commands = new List<ICommand>()
                                {
                                    //await Animations.ScaleInOutOnPress(),
                                    new SendEvent() { arguments = new List<object>() { nameof(UserEventPlaybackStart), room.Name } },
                                }
                            }

                        },
                        new Text()
                        {
                            text = $"{room.Name} " + (disabled ? " (unavailable)" : string.Empty),
                            paddingTop = "12dp",
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
                case "Trailer": return new List<object>() { nameof(UserEventShowBaseItemDetailsTemplate) };
                case "Episode": return new List<object>() { nameof(UserEventPlaybackStart), room };
                default: return new List<object>() { nameof(UserEventShowItemListSequenceTemplate) };
            }
        }
    }
}