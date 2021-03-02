using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.Components;
using AlexaController.Alexa.Presentation.APL.Components.VisualFilters;
using AlexaController.Alexa.Presentation.APL.UserEvent.Pager.Page;
using AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press;
using AlexaController.Alexa.Presentation.APL.VectorGraphics;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using IFilter  = AlexaController.Alexa.Presentation.APL.Components.VisualFilters.IFilter;
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

namespace AlexaController.Alexa
{
    public class RenderDocumentDirectiveManager
    {
        public static RenderDocumentDirectiveManager Instance { get; private set; }

        public RenderDocumentDirectiveManager()
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

        public async Task<IDirective> GetRenderDocumentDirectiveAsync(IDataSource dataSource, IAlexaSession session)
        {
            var properties = dataSource.properties;
            switch (properties.documentType)
            {
                case RenderDocumentType.BROWSE_LIBRARY_TEMPLATE     : return await RenderBrowseLibraryTemplate(dataSource, session);
                case RenderDocumentType.ITEM_DETAILS_TEMPLATE       : return await RenderItemDetailsTemplate(dataSource, session);
                case RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE : return await RenderItemListSequenceTemplate(dataSource, session);
                case RenderDocumentType.FOLLOW_UP_QUESTION          : return await RenderQuestionRequestTemplate(dataSource);
                case RenderDocumentType.ROOM_SELECTION_TEMPLATE     : return await RenderRoomSelectionTemplate(dataSource, session);
                case RenderDocumentType.NOT_UNDERSTOOD              : return await RenderNotUnderstoodTemplate(dataSource);
                case RenderDocumentType.HELP                        : return await RenderHelpTemplate(dataSource);
                case RenderDocumentType.GENERIC_HEADLINE_TEMPLATE   : return await RenderGenericHeadlineRequestTemplate(dataSource);
                case RenderDocumentType.NONE                        : return null;
                default                                             : return null;
            }
        }

        //Create Render Document Template Directives 
        private async Task<IDirective> RenderItemListSequenceTemplate(IDataSource dataSource, IAlexaSession session)
        {
            var layout           = new List<IItem>();
            var properties       = (MediaItemProperties) dataSource.properties;
            var baseItems        = properties.items;
            var type             = baseItems[0].type;
            
            layout.Add(new Container()
            {
                id = "primary",
                width = "100vw",
                height = "100vh",
                items = new List<VisualBaseItem>()
                {
                    new Image()
                    {
                        height = "100%",
                        width = "100%",
                        scale = "best-fill",
                        position = "absolute",
                        source = "${payload.templateData.properties.url}${payload.templateData.properties.item.backdropImageSource}",
                        filter = new List<IFilter>()
                        {
                            new Gradient()
                            {
                                gradient = new GradientOptions()
                                {
                                   type = "linear",
                                   colorRange = new List<string>()
                                   {
                                       "#000",
                                       "rgba(0,0,0,0)"
                                   },
                                   inputRange = new List<double>()
                                   {
                                       0,
                                       0.999
                                   },
                                   angle = 0
                                }
                            },
                           new Blend()
                           {
                               mode = "color-burn",
                               source = -2,
                               destination = -1
                           }
                        }
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
                        items                  = new List<VisualBaseItem>()
                        {
                            new TouchWrapper()
                            {
                                paddingTop = "8vh",
                                id         = "${data.id}",
                                onPress    = new Parallel()
                                {
                                   commands = new List<ICommand>()
                                   {
                                       new Command() { type = nameof(Animations.ScaleInOutOnPress) },
                                       new SendEvent() { arguments = GetSequenceItemsOnPressArguments(type, session) }
                                   }
                               },
                               items = new List<VisualBaseItem>()
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
            
            return await Task.FromResult(new Directive()
            {
                type = Directive.AplRenderDocument,
                token = "mediaItemSequence",
                document = new Document()
                {
                    theme = "dark",
                    import = Imports,
                    onMount = new List<ICommand>(),
                    resources = Resources,
                    commands = new Dictionary<string, ICommand>()
                    {
                        { nameof(Animations.ScaleInOutOnPress), await Animations.ScaleInOutOnPress() }
                    },
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = layout
                    }
                },
                datasources = new Dictionary<string, IDataSource>(){ {"templateData", dataSource } }
            });
        }

        private async Task<IDirective> RenderItemDetailsTemplate(IDataSource dataSource, IAlexaSession session)
        {
            const string leftColumnSpacing = "36vw";
            var properties = (MediaItemProperties) dataSource.properties;
            var baseItem = properties.item;
            var type     = baseItem.type;
            //var item     = type.Equals("Season") ? baseItem.Parent : template.baseItems[0];

            var layout   = new List<VisualBaseItem>();
            const string token = "mediaItemDetails";
            
            layout.Add(new Video()
            {
                source = new List<Source>()
                {
                    new Source()
                    {
                        url         = "${data.url}${data.item.videoBackdropSource}",
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
                                fill        = baseItem.isPlayed ? "rgba(255,0,0,1)" : "white"
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
                }
            };

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
                               type = nameof(Animations.ScaleInOutOnPress)
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
                display = !string.IsNullOrEmpty(baseItem.tagLine) ? "normal" : "none",
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
                top = string.Equals(type, "Movie") ? "24vh" : "20vh",
                left = leftColumnSpacing,
                maxHeight = "20vh",
                opacity = 1,
                id = "${data.item.id}",
                onPress = new SendEvent() { arguments = new List<object>() { nameof(UserEventReadOverview) } },
                item = new Container()
                {
                    items = new List<VisualBaseItem>()
                    {
                        new Container()
                        {
                            direction = "row",
                            items = new List<VisualBaseItem>()
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
                items = new List<VisualBaseItem>()
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
                        items = new List<VisualBaseItem>()
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

                layout.Add(new Container()
                {
                    position = "absolute",
                    left = "38vw",
                    top = "78vh",
                    direction = "row",
                    items = new List<VisualBaseItem>()
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
                                   ServerQuery.Instance.GetItemsResult(baseItem.id, new []{"Season"}, session.User)
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
                items = new List<VisualBaseItem>()
                {
                    new Image()
                    {
                        source = "${data.url}${data.item.primaryImageSource}",
                        scale  = "best-fit",
                        height = "63vh",
                        width  = "100%",
                        id     = "primary"
                    },
                    await RenderComponent_PlayButton(baseItem, session)
                }
            });
            layout.Add(new AlexaFooter()
            {
                hintText = type == "Series" ? "Try \"Alexa, show season one...\"" : "Try \"Alexa, play that...\"",
                position = "absolute",
                bottom = "1vh"
            });

            return await Task.FromResult(new Directive()
            {
                type = Directive.AplRenderDocument,
                token = token,
                document = new Document()
                {
                    theme = "dark",
                    settings = new Settings() { idleTimeout = 120000 },
                    import = Imports,
                    graphics = graphicsDictionary,
                    commands = new Dictionary<string, ICommand>()
                    {
                        { nameof(Animations.ScaleInOutOnPress), await Animations.ScaleInOutOnPress() },
                        { nameof(Animations.FadeIn), await Animations.FadeIn() }
                    },
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                when   = "${viewport.shape == 'round'}",
                                width  = "100vw",
                                height = "100vh",
                                items  = new List<VisualBaseItem>()
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
                                        source = "${data.logoUrl}",//$"{Url}/Items/{template.baseItems[0].InternalId}/Images/logo?quality=90",
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
                                bind = new DataBind()
                                {
                                    name = "data",
                                    value = "${payload.templateData.properties}"
                                },
                                width  = "100vw",
                                height = "100vh",
                                items  = layout,
                            }
                        }
                    }
                },
                datasources = new Dictionary<string, IDataSource>(){ {"templateData", dataSource } }//await DataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync("templateData", baseItem, session)
            });
        }

        private async Task<IDirective> RenderRoomSelectionTemplate(IDataSource dataSource, IAlexaSession session)
        {
            var layout = new List<VisualBaseItem>
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

            var view = new Directive()
            {
                type = Directive.AplRenderDocument,
                token = "roomSelection",
                document = new Document()
                {
                    theme = "dark",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width = "100vw",
                                bind = new DataBind()
                                {
                                    name  = "data",
                                    value = "${payload.templateData.properties}"
                                },
                                items = layout
                            }
                        }
                    }
                },
                datasources = new Dictionary<string, IDataSource>(){ {"templateData", dataSource } }
            };

            return await Task.FromResult(view);
        }

        private async Task<IDirective> RenderBrowseLibraryTemplate(IDataSource dataSource, IAlexaSession session)
        {
            var layout = new List<VisualBaseItem>
            {
                new AlexaBackground()
                {
                    backgroundVideoSource = new List<Source>()
                    {
                        new Source()
                        {
                            url = "${payload.templateData.properties.url}/MoviesLibrary",
                            repeatCount = 1,
                        }
                    },
                    backgroundScale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    videoAutoPlay = true
                },
                new AlexaHeadline()
                {
                    primaryText = "Now showing ${payload.templateData.properties.HeadlinePrimaryText}",
                    secondaryText = $"{session.room.Name}",
                    backgroundColor = "rgba(0,0,0,0.45)"
                }
            };


            return await Task.FromResult(new Directive()
            {
                type = Directive.AplRenderDocument,
                token = "browseLibrary",
                document = new Document()
                {
                    theme = "light",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width  = "100vw",
                                height = "100vh",
                                items  = layout
                            }
                        }
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { { "templateData", dataSource }}
            });
        }

        private async Task<IDirective> RenderQuestionRequestTemplate(IDataSource dataSource)
        {
            var layout = new List<VisualBaseItem>
            {
                new Video()
                {
                    source = new List<Source>()
                    {
                        new Source()
                        {
                            url = "${payload.templateData.properties.url}/Question", repeatCount = 1,
                        }
                    },
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    autoplay = true,
                    audioTrack = "none"
                },
                new Image()
                {
                    overlayColor = "rgba(0,0,0,1)",
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    source = "${payload.templateData.properties.url}/EmptyPng?quality=90",
                    opacity = 0.45
                },
                new AlexaHeadline()
                {
                    backgroundColor = "rgba(0,0,0,0.1)",
                    primaryText = "${payload.templateData.properties.HeadlinePrimaryText}"
                }
            };


            return await Task.FromResult(new Directive()
            {
                type = Directive.AplRenderDocument,
                token = "",
                document = new Document()
                {
                    theme = "light",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width  = "100vw",
                                height = "100vh",
                                items  = layout
                            }
                        }
                    }
                }, 
                datasources = new Dictionary<string, IDataSource>() { { "templateData", dataSource } }
            });
        }

        private async Task<IDirective> RenderNotUnderstoodTemplate(IDataSource dataSource)
        {
            var layout = new List<VisualBaseItem>
            {
                new Video()
                {
                    source = new List<Source>()
                    {
                        new Source()
                        {
                            url = "${payload.templateData.properties.url}/particles", repeatCount = 1,
                        }
                    },
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    autoplay = true,
                    audioTrack = "none"
                },
                new Image()
                {
                    overlayColor = "rgba(0,0,0,1)",
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    source = "${payload.templateData.properties.url}/EmptyPng?quality=90",
                    opacity = 0.35
                },
                new AlexaHeadline()
                {
                    backgroundColor = "rgba(0,0,0,0.1)", primaryText = "Could you say that again?"
                },
                new AlexaFooter() {hintText = "Alexa, open help...", position = "absolute", bottom = "1vh"}
            };


            return await Task.FromResult(new Directive()
            {
                type = Directive.AplRenderDocument,
                token = "",
                document = new Document()
                {
                    theme = "light",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width  = "100vw",
                                height = "100vh",
                                items  = layout
                            }
                        }
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { {"templateData", dataSource } }
            });
        }

        private async Task<IDirective> RenderGenericHeadlineRequestTemplate(IDataSource dataSource)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var layout = new List<VisualBaseItem>();
            var url = await ServerQuery.Instance.GetLocalApiUrlAsync();
            layout.Add(new Video()
            {
                source = new List<Source>()
                {
                    new Source()
                    {
                        url         = $"{url}/particles",
                        repeatCount = 1,
                    }
                },
                scale = "best-fill",
                width = "100vw",
                height = "100vh",
                position = "absolute",
                autoplay = true,
                audioTrack = "none"
            });

            layout.Add(new Image()
            {
                overlayColor = "rgba(0,0,0,1)",
                scale = "best-fill",
                width = "100vw",
                height = "100vh",
                position = "absolute",
                source = $"{url}/EmptyPng?quality=90",
                opacity = 0.35
            });

            layout.Add(new AlexaHeadline()
            {
                backgroundColor = "rgba(0,0,0,0.1)",
                primaryText = "${payload.templateData.properties.HeadlinePrimaryText}"
            });

            layout.Add(new AlexaFooter()
            {
                hintText = "Alexa, open help...",
                position = "absolute",
                bottom = "1vh"
            });

            return await Task.FromResult( new Directive()
            {
                type = Directive.AplRenderDocument,
                token = "",
                document = new Document()
                {
                    theme = "light",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width  = "100vw",
                                height = "100vh",
                                items  = layout
                            }
                        }
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { { "templateData", dataSource } }
            });
        }

        private async Task<IDirective> RenderHelpTemplate(IDataSource dataSource)
        {
            var graphicsDictionary = new Dictionary<string, AlexaVectorGraphic>
            {
                {
                    "AlexaLarge", new AlexaVectorGraphic()
                    {
                        height         = 235,
                        width          = 235,
                        viewportHeight = 25,
                        viewportWidth  = 25,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    = MaterialVectorIcons.Alexa,
                                stroke      = "none",
                                strokeWidth = 0,
                                fill        = "rgba(20,200,255,1)",
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
                        height         = 240,
                        width          = 240,
                        viewportHeight = 25,
                        viewportWidth  = 25,
                        items          = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData    = MaterialVectorIcons.EmbyIcon,
                                stroke      = "none",
                                strokeWidth = 0.1,
                                fill        = "rgba(81,201,39)",
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
                token = "Help",
                document = new Document()
                {
                    theme = "dark",
                    import = Imports,
                    resources = Resources,
                    graphics = graphicsDictionary,
                    commands = new Dictionary<string, ICommand>()
                    {
                        { nameof(Animations.FadeIn), await Animations.FadeIn() }
                    },
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width  = "100vw",
                                height = "100vh",
                                id     = "helpPageContainer",
                                items  = new List<VisualBaseItem>()
                                {
                                    new Frame()
                                    {
                                        id = "logoFrame",
                                        width = "100vw",
                                        height ="100vh",
                                        backgroundColor = "white",
                                        items = new List<VisualBaseItem>()
                                        {
                                            new Container()
                                            {
                                                direction = "row",
                                                alignItems = "center",
                                                justifyContent = "center",
                                                width = "100vw",
                                                height = "100vh",
                                                items = new List<VisualBaseItem>()
                                                {
                                                    new VectorGraphic()
                                                    {
                                                        source = "AlexaLarge",
                                                        id = "logoAlexa",
                                                        width = "50vw",
                                                        height = "50vh",
                                                        opacity = 1
                                                    },
                                                    new VectorGraphic()
                                                    {
                                                        source = "EmbyLarge",
                                                        position = "absolute",
                                                        width = "30vw",
                                                        height = "50vh",
                                                        id = "logoEmby",
                                                        opacity = 0
                                                    }
                                                },
                                                onMount = new List<ICommand>()
                                                {
                                                    new Sequential()
                                                    {
                                                        commands = new List<ICommand>()
                                                        {
                                                            new AnimateItem()
                                                            {
                                                                easing = "ease-in",
                                                                componentId = "logoAlexa",
                                                                duration = 1000,
                                                                delay = 2000,
                                                                value = new List<IValue>()
                                                                {
                                                                    new OpacityValue()
                                                                    {
                                                                        @from = 1,
                                                                        to = 0
                                                                    }
                                                                }
                                                            },
                                                            new AnimateItem()
                                                            {
                                                                easing = "ease-in",
                                                                componentId = "logoEmby",
                                                                duration = 1000,
                                                                value = new List<IValue>()
                                                                {
                                                                    new OpacityValue()
                                                                    {
                                                                        @from = 0,
                                                                        to = 1
                                                                    }
                                                                }
                                                            },
                                                            new AnimateItem()
                                                            {
                                                                easing = "ease-in",
                                                                componentId = "logoFrame",
                                                                duration = 1000,
                                                                delay = 2000,
                                                                value = new List<IValue>()
                                                                {
                                                                    new OpacityValue()
                                                                    {
                                                                        @from = 1,
                                                                        to = 0
                                                                    }
                                                                }
                                                            },
                                                            new SetValue()
                                                            {
                                                                componentId = "logoFrame",
                                                                property = "display",
                                                                value = "none"
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
                                        data          = "${payload.templateData.properties.helpContent}",
                                        items         = new List<VisualBaseItem>()
                                        {
                                            new Container()
                                            {
                                                justifyContent = "center",
                                                alignItems = "center",
                                                direction = "column",
                                                items = new List<VisualBaseItem>()
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
                                                        id = "helpText",
                                                        textAlign         = "center",
                                                        textAlignVertical = "center",
                                                        paddingRight      = "20dp",
                                                        paddingLeft       = "20dp",
                                                        speech = "${data.speech.url}"
                                                    }
                                                }
                                            }
                                        },
                                        id = "HelpPager",
                                        onPageChanged = new List<ICommand>()
                                        {
                                            new SendEvent() { arguments = new List<object>() { nameof(HelpPager),  "${payload.templateData.properties.helpContent[event.source.value].value}" } }
                                        }
                                    }
                                }

                            }
                        }
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { { "templateData", dataSource } }//await DataSourceManager.Instance.GetHelpDataSourceAsync("templateData")
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
                items = new List<VisualBaseItem>()
                {
                    new AlexaIconButton()
                    {
                        vectorSource = item.type == "Series" ?  MaterialVectorIcons.ListIcon : MaterialVectorIcons.PlayOutlineIcon,
                        buttonSize = "13vh",
                        id = item.id.ToString(),
                        primaryAction = new Parallel()
                        {
                            commands = new List<ICommand>()
                            {
                                new Command()   { type = nameof(Animations.ScaleInOutOnPress) },
                                new SendEvent() { arguments = args }
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
                        items = new List<VisualBaseItem>()
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
                        items = new List<VisualBaseItem>()
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

        private List<VisualBaseItem> RenderComponent_RoomButtonLayoutContainer()
        {
            var config = Plugin.Instance.Configuration;
            var roomButtons = new List<VisualBaseItem>();

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
                    items = new List<VisualBaseItem>()
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