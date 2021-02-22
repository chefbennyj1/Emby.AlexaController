using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.Components;
using AlexaController.Alexa.Presentation.APL.UserEvent.Pager.Page;
using AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press;
using AlexaController.Alexa.Presentation.APL.VectorGraphics;
using AlexaController.Alexa.Presentation.DataSource;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;
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

namespace AlexaController.Alexa.Presentation.DirectiveBuilders
{
    public class RenderDocumentBuilder 
    {
        public static RenderDocumentBuilder Instance { get; private set; }
     
        public RenderDocumentBuilder()
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
                colors = new Colors()
                {
                    colorTextPrimary = "#151920"
                }
            },
            new Resource()
            {
                description = "Stock color for the dark theme",
                when = "${viewport.theme == 'dark'}",
                colors = new Colors()
                {
                    colorTextPrimary = "#f0f1ef"
                }
            },
            new Resource()
            {
                description = "Standard font sizes",
                dimensions = new Dimensions()
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
                dimensions = new Dimensions()
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
                dimensions = new Dimensions()
                {
                    marginTop    = 40,
                    marginLeft   = 60,
                    marginRight  = 60,
                    marginBottom = 40
                }
            }
        };
        
        public async Task<IDirective> GetRenderDocumentDirectiveAsync(RenderDocumentTemplate template, IAlexaSession session)
        {
           
            switch (template.renderDocumentType)
            {
                case RenderDocumentType.BROWSE_LIBRARY_TEMPLATE     : return await RenderBrowseLibraryTemplate(template, session);
                case RenderDocumentType.ITEM_DETAILS_TEMPLATE       : return await RenderItemDetailsTemplate(template, session);
                case RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE : return await RenderItemListSequenceTemplate(template, session);
                case RenderDocumentType.QUESTION_TEMPLATE           : return await RenderQuestionRequestTemplate(template);
                case RenderDocumentType.ROOM_SELECTION_TEMPLATE     : return await RenderRoomSelectionTemplate(template, session);
                case RenderDocumentType.NOT_UNDERSTOOD              : return await RenderNotUnderstoodTemplate();
                case RenderDocumentType.VERTICAL_TEXT_LIST_TEMPLATE : return await RenderVerticalTextListTemplate(template, session);
                case RenderDocumentType.HELP                        : return await RenderHelpTemplate();
                case RenderDocumentType.GENERIC_HEADLINE_TEMPLATE   : return await RenderGenericHeadlineRequestTemplate(template);
                case RenderDocumentType.NONE                        : return null;
                default                                             : return null;
            }
        }

        //Create Render Document Template Directives 
        private async Task<IDirective> RenderItemListSequenceTemplate(RenderDocumentTemplate template, IAlexaSession session)
        {
            ServerController.Instance.Log.Info("Render Document Started");

            var layout           = new List<IItem>();
            var baseItems        = template.baseItems;
            var type             = baseItems[0].GetType().Name;
            var url              = await ServerQuery.Instance.GetLocalApiUrlAsync();
            var attributionImage = template.HeaderAttributionImage != null ? $"{url}{template.HeaderAttributionImage}" : "";

            ServerController.Instance.Log.Info($"Render Document is RenderItemListSequenceTemplate for {type}");
            
            layout.Add(new Container()
            {
                id     = "primary",
                width  = "100vw",
                height = "100vh",
                items  = new List<VisualItem>()
                {
                    new AlexaHeader()
                    {
                        headerTitle            = $"{template.HeaderTitle}",
                        headerBackButton       = session.paging.canGoBack,
                        headerDivider          = true,
                        headerAttributionImage = attributionImage
                    },
                    new Sequence()
                    {
                        height                 = "100vh",
                        width                  = "95vw",
                        top                    = "4vh",
                        left                   = "5vw",
                        scrollDirection        = "horizontal",
                        data                   = "${payload.templateData.properties.items}",
                        items                  = new List<VisualItem>()
                        {
                            new TouchWrapper()
                            {
                               id      = "${data.id}",
                               onPress = new Parallel()
                               {
                                   commands = new List<ICommand>()
                                   {
                                       new Command() { type = nameof(Animations.ScaleInOutOnPress) },
                                       new SendEvent() { arguments = GetSequenceItemsOnPressArguments(type, session) }
                                   }
                               },
                               items = new List<VisualItem>()
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
                        onMount = new List<ICommand>()
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
                                        value       =  "Try \"Alexa, Show The ${payload.templateData.properties.items[0].type} ${payload.templateData.properties.items[Time.seconds(localTime/payload.templateData.properties.items.length) % payload.templateData.properties.items.length].name}\"",
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

            ServerController.Instance.Log.Info("Render Document has Item Touch Wrapper Containers");
            ServerController.Instance.Log.Info("Render Document has Sequence Item Hint Texts");
            ServerController.Instance.Log.Info("Render Document is creating view");
            
            var view = new Directive()
            {
                type  = "Alexa.Presentation.APL.RenderDocument",
                token = "mediaItemSequence",
                document = new Document()
                {
                    theme     = "dark",
                    import    = Imports,
                    onMount   = new List<ICommand>(),
                    resources = Resources,
                    commands  = new Dictionary<string, ICommand>()
                    {
                        { nameof(Animations.ScaleInOutOnPress), await Animations.ScaleInOutOnPress() }
                    },
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = layout
                    }
                },
                datasources = await GetSequenceItemsDataSource("templateData", baseItems)
            };

            ServerController.Instance.Log.Info("Render Document sequence view is ready");

            return await Task.FromResult(view);
        }
        
        private async Task<IDirective> RenderItemDetailsTemplate(RenderDocumentTemplate template, IAlexaSession session)
        {
            ServerController.Instance.Log.Info("Render Document Started");

            const string leftColumnSpacing = "36vw";

            var baseItem = template.baseItems[0];
            var type     = baseItem.GetType().Name;
            var item     = type.Equals("Season") ? baseItem.Parent : template.baseItems[0];
            
            var layout = new List<VisualItem>();
            const string token = "mediaItemDetails";

            ServerController.Instance.Log.Info($"Render Document is {token} for {item.Name}");

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
                scale      = "best-fill",
                width      = "100vw",
                height     = "100vh",
                position   = "absolute",
                autoplay   = true,
                audioTrack = "none",
                id         = "${data.item.id}",
                onEnd      = new List<ICommand>()
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
                scale        = "best-fill",
                width        = "100vw",
                height       = "100vh",
                position     = "absolute",
                source       = "${data.url}${data.item.videoOverlaySource}",
                opacity      = 0.65,
                id           = "backdropOverlay"
            });

            //(await RenderComponent_VideoBackdrop(item, token)).ForEach(i => layout.Add(i));

            ServerController.Instance.Log.Info($"Render Document has {layout.Count} video backdrops");
            
            var graphicsDictionary = new Dictionary<string, AlexaVectorGraphic>
            {
                {
                    "CheckMark", new AlexaVectorGraphic()
                    {
                        height = 35,
                        width = 35,
                        viewportHeight = 25,
                        viewportWidth = 25,
                        items = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData = MaterialVectorIcons.CheckMark,
                                stroke = "none",
                                strokeWidth = 1,
                                fill = template.baseItems[0].IsPlayed(session.User) ? "rgba(255,0,0,1)" : "white"
                            }
                        }
                    }
                },
                {
                    "Audio", new AlexaVectorGraphic()
                    {
                        height = 25,
                        width = 25,
                        viewportHeight = 28,
                        viewportWidth = 28,
                        items = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData = MaterialVectorIcons.Audio,
                                stroke = "none",
                                strokeWidth = 1,
                                fill = "white"
                            }
                        }
                    }
                },
                {
                    "Carousel", new AlexaVectorGraphic()
                    {
                        height = 35,
                        width = 35,
                        viewportHeight = 25,
                        viewportWidth = 25,
                        items = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData =  MaterialVectorIcons.Carousel,
                                stroke = "none",
                                strokeWidth = 1,
                                fill = "rgba(255,250,0,1)" 
                            }
                        }
                    }
                },
                {
                    "ArrayIcon", new AlexaVectorGraphic()
                    {
                        height = 35,
                        width = 35,
                        viewportHeight = 25,
                        viewportWidth = 25,
                        items = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData =  MaterialVectorIcons.ArrayIcon,
                                stroke = "none",
                                strokeWidth = 1,
                                fill = "rgba(255,250,0,1)" 
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
                id       = "logo",
                source   = "${data.url}${data.item.logoImageSource}", 
                width    = "12vw",
                position = "absolute",
                left     = "85vw",
            });
            ServerController.Instance.Log.Info("Render Document has Logo");

            //Name
            layout.Add(new Text()
            {
                text       = "${data.item.name}", 
                style      = "textStylePrimary",
                left       = leftColumnSpacing,
                fontWeight = "100",
                top        = "15vh",
                id         = "baseItemName",
                opacity    = 0
            });
            ServerController.Instance.Log.Info("Render Document has Rating");

            //Genres
            layout.Add(new Text()
            {
                // ReSharper disable once TooManyChainedReferences
                text     = "${data.item.genres}", 
                left     = leftColumnSpacing,
                style    = "textStyleBody",
                top      = "15vh",
                width    = "40vw",
                height   = "22dp",
                fontSize = "18dp",
                opacity  = 0,
                id       = "genre",
            });
            ServerController.Instance.Log.Info("Render Document has Genres");

            //Rating - Runtime - End time
            //Runtime span
            layout.Add(new Text()
            {
                // ReSharper disable once TooManyChainedReferences
                text     = "${data.item.premiereDate} | ${data.item.officialRating} | ${data.item.runtimeMinutes} | ${data.item.endTime}",  
                left     = leftColumnSpacing,
                style    = "textStyleBody",
                top      = "17vh",
                width    = "40vw",
                height   = "22dp",
                fontSize = "18dp",
                opacity  = 0,
                id       = "rating",
            });
            ServerController.Instance.Log.Info("Render Document has Genres");

            //TagLines
            layout.Add(new Text()
            {
                text     = "${data.item.tagLine}", 
                style    = "textStyleBody",
                left     = leftColumnSpacing,
                top      = "18vh",
                height   = "10dp",
                width    = "40vw",
                fontSize = "22dp",
                id       = "tag",
                display  = !string.IsNullOrEmpty(item.Tagline) ? "normal" : "none",
                opacity  = 0
            });
            ServerController.Instance.Log.Info("Render Document has Tag lines");

            //Watched check-mark
            layout.Add(new VectorGraphic()
            {
                source   = "CheckMark",
                left     = "87vw",
                position = "absolute",
                top      = "30vh"
            });
            ServerController.Instance.Log.Info("Render Document has Watch status");

            
            ServerController.Instance.Log.Info("Render Document has Runtime");

            //Overview
            layout.Add(new TouchWrapper()
            {
                top     = string.Equals(type, "Movie") ? "24vh" : "20vh",
                left    = leftColumnSpacing,
                height  = "20vh",
                opacity = 1,
                id      = "${data.item.id}",//baseItem.InternalId.ToString(),
                onPress = new SendEvent() { arguments = new List<object>() { nameof(UserEventReadOverview) }},
                item    = new Container()
                {
                    items = new List<VisualItem>()
                    {
                        new Container()
                        {
                            direction = "row",
                            items = new List<VisualItem>()
                            {
                                new Text()
                                {
                                    fontSize = "22dp",
                                    text     = "<b>Overview</b>",
                                    style    = "textStyleBody",
                                    width    = "35vw",
                                    id       = "overviewHeader",
                                    opacity  = 0
                                },
                                new VectorGraphic()
                                {
                                    source  = "Audio",
                                    right   = "25vw",
                                    opacity = 0,
                                    id      = "audioIcon",
                                    top     = "5px"
                                }
                            }
                        },
                        new Text()
                        {
                            text     = "${data.item.overview}", 
                            style    = "textStyleBody",
                            id       = "overview",
                            width    = "55vw",
                            fontSize = "20dp",
                            opacity  = 0
                        }
                    }
                }
            });
            ServerController.Instance.Log.Info("Render Document has Overview");

            //Room
            layout.Add(new Text()
            {
                text = $"{(session.room != null ? session.room.Name.ToUpperInvariant() : string.Empty)}",
                id = "rating",
                style = "textStyleBody",
                top = "22vh",
                left = leftColumnSpacing,
                opacity = 0
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
                    items = new List<VisualItem>()
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
                            opacity = 0
                        },
                        new Text()
                        {
                            style = "textStyleBody",
                            fontSize = "23dp",
                            left = "12dp",
                            text = "<b>" + 
                                   ServerQuery.Instance.GetItemsResult(item, new []{"Season"}, session.User)
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
                id       = "primaryButton",
                position = "absolute",
                width    = "38%",
                height   = "75vh",
                top      = "15vh",
                opacity  = 1,
                items    = new List<VisualItem>()
                {
                    new Image()
                    {
                        source = "${data.url}${data.item.primaryImageSource}",
                        scale  = "best-fit",
                        height = "63vh",
                        width  = "100%",
                        id     = "primary"
                    },
                    
                        // ReSharper disable once ComplexConditionExpression
                         await RenderComponent_ButtonFrame(args : type == "Movie" || type == "Episode" 
                                ? new List<object>() { nameof(UserEventPlaybackStart), session.room != null ? session.room.Name : "" } 
                                : new List<object>() { nameof(UserEventShowItemListSequenceTemplate) },
                                   icon : item.GetType().Name == "Series" ?  MaterialVectorIcons.ListIcon : MaterialVectorIcons.PlayOutlineIcon,
                                   id   : template.baseItems[0].InternalId.ToString())
                }
            });
            ServerController.Instance.Log.Info("Render Document has Primary Image");
            
            layout.Add(new AlexaFooter()
            {
                hintText = type == "Series" ? "Try \"Alexa, show season one...\"" : "Try \"Alexa, play that...\"",
                position = "absolute",
                bottom = "1vh"
            });
            ServerController.Instance.Log.Info("Render Document has Footer");

            // ReSharper disable once ComplexConditionExpression
            var view = new Directive()
            {
                type     = "Alexa.Presentation.APL.RenderDocument",
                token    = token,
                document = new Document()
                {
                    theme    = "dark",
                    settings = new Settings() { idleTimeout = 120000 },
                    import   = Imports,
                    graphics = graphicsDictionary,
                    commands = new Dictionary<string, ICommand>()
                    {
                        { nameof(Animations.ScaleInOutOnPress), await Animations.ScaleInOutOnPress() }
                    },
                    onMount = new List<ICommand>()
                    {
                        new Sequential()
                        {
                            commands = new List<ICommand>()
                            {
                                new Parallel()
                                {
                                    commands = new List<ICommand>()
                                    {
                                        await Animations.FadeIn("primaryButton", 1250),
                                        await Animations.FadeIn("baseItemName", 1250),
                                        await Animations.FadeIn("SeasonCarouselIcon", 500),
                                        await Animations.FadeOutItem("SeasonCarouselArrayIcon", 500)
                                    }
                                },
                                await Animations.FadeIn("genre", 1250),
                                await Animations.FadeIn("rating", 1250),
                                await Animations.FadeIn("tag", 50),
                                new Parallel()
                                {
                                    commands = new List<ICommand>()
                                    {
                                        await Animations.FadeIn("overviewHeader", 1250),
                                        await Animations.FadeIn("overview", 1250),
                                        await Animations.FadeIn("audioIcon", 1250),
                                    }
                                },
                            }
                        },
                        //session.PlaybackStarted == true ? new Sequential()
                        //{
                        //    commands = new List<ICommand>()
                        //    {
                        //        new SendEvent()
                        //        {
                        //            arguments = new List<object>() { nameof(PlaybackProgressValueUpdate), token}
                        //        }
                        //    },
                        //    delay = 1000,
                        //    repeatCount = (int)(session.NowViewingBaseItem.RunTimeTicks / 20000)
                        //} : null
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
                                items  = new List<VisualItem>()
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
                datasources = await GetBaseItemDetailsDataSource("templateData", baseItem)
            };
            ServerController.Instance.Log.Info("Render Document is creating view");

            return await Task.FromResult(view);
        }
        
        private async Task<IDirective> RenderVerticalTextListTemplate(RenderDocumentTemplate template, IAlexaSession session)
        {
            var layout          = new List<VisualItem>();
            var layoutBaseItems = new List<VisualItem>();
            var baseItems       = template.baseItems;

            const string token = "textList";

            baseItems.ForEach(item => layoutBaseItems.Add(new TouchWrapper()
            {
                id = item.InternalId.ToString(),
                onPress = new Sequential()
                {
                    commands = new List<ICommand>()
                    {
                        new SendEvent()
                        {
                            arguments =  new List<object>() { nameof(UserEventShowBaseItemDetailsTemplate), session.room.Name }
                        }
                    }
                },
                items = new List<VisualItem>()
                {
                    new Container()
                    {
                        direction   = "row",
                        paddingLeft = "12vw",
                        paddingTop  = "4vh",
                        items = new List<VisualItem>()
                        {
                            new Text()
                            {
                                text = $"{item.Name} ({item.ProductionYear}) - {item.OfficialRating}",
                                paddingLeft = "2vw",
                                paddingTop = "25dp"
                            }
                        }
                    }
                }
            }));

            layout.Add(new AlexaHeader()
            {
                headerTitle = template.HeaderTitle,
                headerDivider = true
            });

            layout.Add(new Sequence()
            {
                scrollDirection = "vertical",
                items = layoutBaseItems,
                grow = 1,
                width = "100%",
                height = "100%",
                paddingBottom = "12vh"
            });

            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = token,
                document = new Document()
                {
                    theme = "dark",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>()
                        {
                            "payload"
                        },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width = "100%",
                                height = "100%",
                                items = layout
                            },
                            new Container()
                            {
                                position = "absolute",
                                items = new List<VisualItem>()
                                {
                                    await RenderComponent_ButtonFrame(new List<object>() {"ShowVerticalTextListTemplate"},  MaterialVectorIcons.Right, "ScrollNext" )
                                }
                            },
                            new Container()
                            {
                                position = "absolute",
                                items = new List<VisualItem>()
                                {
                                    await RenderComponent_ButtonFrame(new List<object>() {"ShowVerticalTextListTemplate"},  MaterialVectorIcons.Left, "ScrollPrev" )
                                }
                            },
                        }
                    }
                }
            };

            return await Task.Factory.StartNew(() => view);
        }
        
        private async Task<IDirective> RenderRoomSelectionTemplate(RenderDocumentTemplate template, IAlexaSession session)
        {
            var url = await ServerQuery.Instance.GetLocalApiUrlAsync();
            var imageEndpoint  = $"/Items/{template.baseItems[0].InternalId}/Images";
            var layout         = new List<VisualItem>();
            const string token = "roomSelection";

            (await RenderComponent_VideoBackdrop(template.baseItems[0], token)).ForEach(b => layout.Add(b));
            
            layout.Add(new AlexaHeader()
            {
                headerBackButton = true,
                headerTitle = $"{template.baseItems[0].Name}",
                headerSubtitle = $"{session.User.Name} Play On...",
                headerDivider = true
            });

            layout.Add(new Image()
            {
                position = "absolute",
                source = url + imageEndpoint + "/logo?quality=90",
                width = "25vw",
                height = "10vh",
                right = "5vw",
                bottom = "5vh"
            });

            var roomButtons = RenderComponent_RoomButtonLayoutContainer(template);
            roomButtons.ForEach(b => layout.Add(b));
            
            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = token,
                document = new Document()
                {
                    theme = "dark",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>()
                        {
                            "payload"
                        },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width = "100vw",
                                items = layout
                            }
                        }
                    }
                }
            };

            return await Task.FromResult(view);
        }

        private async Task<IDirective> RenderBrowseLibraryTemplate(RenderDocumentTemplate template, IAlexaSession session)
        {
            var url = await ServerQuery.Instance.GetLocalApiUrlAsync();
            var layout = new List<VisualItem>();
            const string token = "browseLibrary";

            layout.Add(new AlexaBackground()
            {
                backgroundVideoSource = new List<Source>()
                {
                    new Source()
                    {
                        url         = $"{url}/MoviesLibrary",
                        repeatCount = 1,
                    }
                },
                backgroundScale = "best-fill",
                width = "100vw",
                height = "100vh",
                position = "absolute",
                videoAutoPlay = true
            });

            layout.Add(new AlexaHeadline()
            {
                primaryText = $"Now showing {template.baseItems[0].Name}",
                secondaryText = $"{session.room.Name}",
                backgroundColor = "rgba(0,0,0,0.45)"
            });

            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = token,
                document = new Document()
                {
                    theme = "light",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>()
                        {
                            "payload"
                        },
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
                }
            };

            return await Task.FromResult<IDirective>(view);
        }

        private async Task<IDirective> RenderQuestionRequestTemplate(RenderDocumentTemplate template)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var layout = new List<VisualItem>();
            var url = await ServerQuery.Instance.GetLocalApiUrlAsync();
            layout.Add(new Video()
            {
                source = new List<Source>()
                {
                    new Source()
                    {
                        url         = $"{url}/Question",
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
                scale        = "best-fill",
                width        = "100vw",
                height       = "100vh",
                position     = "absolute",
                source       = $"{url}/EmptyPng?quality=90",
                opacity      = 0.45
            });

            layout.Add(new AlexaHeadline()
            {
                backgroundColor = "rgba(0,0,0,0.1)",
                primaryText = template.HeadlinePrimaryText
            });

            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = "",
                document = new Document()
                {
                    theme = "light",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>()
                        {
                            "payload"
                        },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width  = "100vw",
                                height = "100vh",
                                items =  layout
                            }
                        }
                    }
                }
            };

            return await Task.FromResult<IDirective>(view);
        }

        private async Task<IDirective> RenderNotUnderstoodTemplate()
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var layout = new List<VisualItem>();
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
                primaryText = "Could you say that again?"
            });

            layout.Add(new AlexaFooter()
            {
                hintText = "Alexa, open help...",
                position = "absolute",
                bottom = "1vh"
            });

            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = "",
                document = new Document()
                {
                    theme = "light",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>()
                        {
                            "payload"
                        },
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
                }
            };

            return await Task.FromResult<IDirective>(view);
        }

        private async Task<IDirective> RenderGenericHeadlineRequestTemplate(RenderDocumentTemplate template)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            var layout = new List<VisualItem>();
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
                primaryText = template.HeadlinePrimaryText
            });

            layout.Add(new AlexaFooter()
            {
                hintText = "Alexa, open help...",
                position = "absolute",
                bottom = "1vh"
            });

            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = "",
                document = new Document()
                {
                    theme = "light",
                    import = Imports,
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>()
                        {
                            "payload"
                        },
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
                }
            };

            return await Task.FromResult<IDirective>(view);
        }
        
        private async Task<IDirective> RenderHelpTemplate()
        {
            var helpItems = new List<VisualItem>();
            var url = await ServerQuery.Instance.GetLocalApiUrlAsync();
            var graphicsDictionary = new Dictionary<string, AlexaVectorGraphic>
            {
                {
                    "Emby", new AlexaVectorGraphic()
                    {
                        height = 235,
                        width = 235,
                        viewportHeight = 25,
                        viewportWidth = 25,
                        items = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData = MaterialVectorIcons.EmbyIcon,
                                stroke = "rgba(81,201,39)",
                                strokeWidth = 0,
                                fill = "rgb(81,201,39)"
                            }
                        }
                    }
                },
                {
                    "Line", new AlexaVectorGraphic()
                    {
                        height = 55,
                        width = 500,
                        viewportWidth = 50,
                        viewportHeight = 50,
                        items = new List<IVectorGraphic>()
                        {
                            new VectorPath()
                            {
                                pathData = "M0 0 l1120 0",
                                stroke = "rgba(255,255,255)",
                                strokeWidth = 1
                            }
                        }
                    }
                }
            };
            
            helpItems.Add(new Container()
            {
                justifyContent = "center",
                alignItems = "center",
                direction = "column",
                items = new List<VisualItem>()
                {
                    new VectorGraphic()
                    {
                        source = "Emby",
                        id = "embyLogo",
                        onMount = new List<ICommand>()
                        {
                            Animations.FadeIn("embyLogo", 1200).Result
                        }
                    },
                    new Text()
                    {
                       text       = "emby",
                       fontSize   = "100",
                       fontFamily = "Roboto",
                       fontWeight = "300",
                       id         = "embyText",
                       top        = "-4vh"
                    }
                }
            });

            RenderAudioBuilder.HelpStrings.ForEach(s => helpItems.Add(new Container()
            {
                justifyContent = "center",
                alignItems = "center",
                direction = "column",
                items = new List<VisualItem>()
                {
                    new VectorGraphic()
                    {
                        source = "Line",
                    },
                    new Text()
                    {
                        id                = "helpText",
                        text              = s,
                        textAlign         = "center",
                        textAlignVertical = "center",
                        paddingRight      = "20dp",
                        paddingLeft       = "20dp"
                    }
                }
            }));

            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = "Help",
                document = new Document()
                {
                    theme = "dark",
                    import = Imports,
                    resources = Resources,
                    graphics = graphicsDictionary,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>()
                        {
                            "payload"
                        },
                        items = new List<IItem>()
                        {
                            new Container()
                            {
                                width  = "100vw",
                                height = "100vh",
                                items  = new List<VisualItem>()
                                {
                                    new AlexaBackground()
                                    {
                                        backgroundVideoSource = new List<Source>()
                                        {
                                            new Source()
                                            {
                                                repeatCount = -1,
                                                url         = $"{url}/MovingFloor"
                                            }
                                        },
                                        backgroundScale = "best-fill",
                                        videoAudioTrack = "none",
                                        videoAutoPlay   = true,
                                        overlayGradient = true,
                                        colorOverlay = true
                                    },
                                    new Pager()
                                    {
                                        height      = "100vh",
                                        width       = "100vw",
                                        initialPage = 0,
                                        navigation  = "forward-only",
                                        items       = helpItems,
                                        id          = "HelpPager",
                                        onPageChanged = new List<object>()
                                        {
                                            new SendEvent() { arguments = new List<object>() { nameof(HelpPager), "Help", "${event.source.value}" } }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            };

            return await Task.FromResult<IDirective>(view);
        }


        //Create template components 
        private static async Task<Frame> RenderComponent_ButtonFrame(List<object> args, string icon, string id = "")
        {
            return await Task.FromResult(new Frame()
            {
                position = "absolute",
                top = "29vh",
                right = "40%",
                borderWidth = "3px",
                borderColor = "white",
                borderRadius = "75px",
                backgroundColor = "rgba(0,0,0,0.35)",
                items = new List<VisualItem>()
                {
                    new AlexaIconButton()
                    {
                        vectorSource = icon,
                        buttonSize = "13vh",
                        id = id,
                        primaryAction = new Parallel()
                        {
                            commands = new List<ICommand>()
                            {
                                await Animations.ScaleInOutOnPress(),
                                new SendEvent() {arguments = args}
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
                case "Episode" : 
                    return await Task.FromResult(new Container()
                    {
                        height = "70vh",
                        width  = "30vw",
                        items  = new List<VisualItem>()
                        {
                            new Image()
                            {
                                source       = "${payload.templateData.properties.url}.${data.primaryImageSource}",
                                width        = "30vw",
                                height       = "62vh",
                                paddingRight = "12px",
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
                        items = new List<VisualItem>()
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

        private List<VisualItem> RenderComponent_RoomButtonLayoutContainer(RenderDocumentTemplate template)
        {
            var config = Plugin.Instance.Configuration;
            var roomButtons = new List<VisualItem>();

            if (config.Rooms is null) return roomButtons;

            System.Threading.Tasks.Parallel.ForEach(config.Rooms, room =>
            {
                var disabled = true;

                foreach (var session in ServerQuery.Instance.GetCurrentSessions())
                {
                    if (session.DeviceName == room.DeviceName) disabled = false;
                    if (session.Client     == room.DeviceName) disabled = false;
                }

                roomButtons.Add(new Container()
                {
                    direction = "row",
                    left = "15vw",
                    top = "10vh",
                    items = new List<VisualItem>()
                    {
                        new AlexaIconButton()
                        {
                            id = template.baseItems[0].InternalId.ToString(),
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
        
        private async Task<List<VisualItem>> RenderComponent_VideoBackdrop(BaseItem baseItem, string token)
        {
            var videoBackdropIds = baseItem.ThemeVideoIds;
            // ReSharper disable once TooManyChainedReferences
            var videoBackdropId = videoBackdropIds.Length > 0 ? ServerQuery.Instance.GetItemById(videoBackdropIds[0]).InternalId.ToString() : string.Empty;
            var url = await ServerQuery.Instance.GetLocalApiUrlAsync();
            var backdropImageUrl     = $"{url}/Items/{baseItem.InternalId}/Images/backdrop?maxWidth=1200&amp;maxHeight=800&amp;quality=90";
            var videoBackdropUrl     = $"{url}/videos/{videoBackdropId}/stream.mp4";
            var videoBackdropOverlay = $"{url}/EmptyPng?quality=90";

           
            if (!string.IsNullOrEmpty(videoBackdropId))
            {
                return await Task.FromResult(new List<VisualItem>()
                {
                    new Video()
                    {
                        source = new List<Source>()
                        {
                            new Source()
                            {
                                url         = "${data.videoBackdropSource}",
                                repeatCount = 0,
                            }
                        },
                        scale = "best-fill",
                        width = "100vw",
                        height = "100vh",
                        position = "absolute",
                        autoplay = true,
                        audioTrack = "none",
                        id = baseItem.InternalId.ToString(),
                        onEnd = new List<ICommand>()
                        {
                            new SetValue()
                            {
                                componentId = "backdropOverlay",
                                property    = "source",
                                value       = "${data.backdropImageSource}"
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
                    },
                    new Image()
                    {
                        overlayColor = "rgba(0,0,0,1)",
                        scale = "best-fill",
                        width = "100vw",
                        height = "100vh",
                        position = "absolute",
                        source = videoBackdropOverlay,
                        opacity = 0.65,
                        id = "backdropOverlay"
                    }
                });
            }

            return await Task.FromResult(new List<VisualItem>()
            {
                new Image()
                {
                    overlayColor = "rgba(0,0,0,0.55)",
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    source = backdropImageUrl
                }
            });
        }

        
       //Data Sources
        private static async Task<Dictionary<string, IDataSource>> GetSequenceItemsDataSource(string dataSourceKey, List<BaseItem> sequenceItems)
        {
            var dataSource      = new Dictionary<string, IDataSource>();
            var dataSourceItems = new List<Item>();
            var type            = sequenceItems[0].GetType().Name;
            sequenceItems.ForEach(i => dataSourceItems.Add(new Item()
            {
                type               = type,
                primaryImageSource = ServerQuery.Instance.GetPrimaryImageUrl(i), 
                id                 = i.InternalId,
                name               = i.Name,
                index              = type == "Episode" ? $"Episode {i.IndexNumber}" : string.Empty,
                premiereDate       = i.PremiereDate?.ToString("D")
            }));

            dataSource.Add(dataSourceKey, new DataSourceObject()
            {
                properties = new Properties()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    items = dataSourceItems
                }
            });
            ServerController.Instance.Log.Info("Render Document Sequence has Data Source");
            return await Task.FromResult(dataSource);
        }

        private static async Task<Dictionary<string, IDataSource>> GetBaseItemDetailsDataSource(string dataSourceKey, BaseItem item)
        {
            var dataSource      = new Dictionary<string, IDataSource>();
            // ReSharper disable once ComplexConditionExpression
            var dataSourceItem = new Item()
            {
                type                = item.GetType().Name,
                primaryImageSource  = ServerQuery.Instance.GetPrimaryImageUrl(item), 
                id                  = item.InternalId,
                name                = item.Name,
                premiereDate        = item.ProductionYear.ToString(),
                officialRating      = item.OfficialRating,
                tagLine             = item.Tagline,
                runtimeMinutes      = ServerQuery.Instance.GetRunTime(item),
                endTime             = ServerQuery.Instance.GetEndTime(item),
                genres              = $"{(item.Genres.Any() ? item.Genres.Aggregate((genres, genre) => genres + ", " + genre) : "")}",
                logoImageSource     = ServerQuery.Instance.GetLogoUrl(item),
                overview            = item.Overview,
                videoBackdropSource = ServerQuery.Instance.GetVideoBackdropUrl(item),
                backdropImageSource = ServerQuery.Instance.GetBackdropImageUrl(item),
                videoOverlaySource  = ServerQuery.Instance.GetVideoOverlay()
            };
            dataSource.Add(dataSourceKey, new DataSourceObject()
            {
                properties = new Properties()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    item = dataSourceItem
                }
            });
            ServerController.Instance.Log.Info("Render Document has Data Source");
            return await Task.FromResult(dataSource);
        }

        //Event Arguments
        private static List<object> GetSequenceItemsOnPressArguments(string type, IAlexaSession session)
        {
            var room = session.room != null ? session.room.Name : "";
            //TODO: Do we want to show episodes Detail page from sequence, or just play the item?
            switch (type)
            {
                case "Movie"   :
                case "Trailer" : return new List<object>() { nameof(UserEventShowBaseItemDetailsTemplate) };
                case "Episode" : return new List<object>() { nameof(UserEventPlaybackStart), room };
                default        : return new List<object>() { nameof(UserEventShowItemListSequenceTemplate) };
            }
        }
    }
}