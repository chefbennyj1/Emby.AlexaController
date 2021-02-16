using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.Components;
using AlexaController.Alexa.Presentation.APL.UserEvent.Pager.Page;
using AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press;
using AlexaController.Alexa.Presentation.APL.UserEvent.Video.End;
using AlexaController.Alexa.Presentation.APL.VectorGraphics;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using Parallel = AlexaController.Alexa.Presentation.APL.Commands.Parallel;
using Source   = AlexaController.Alexa.Presentation.APL.Components.Source;
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
        private static string LocalApiUrl            { get; set; }
      
        public RenderDocumentBuilder()
        {
            Instance       = this;
        }

        private static string Url => $"{LocalApiUrl}/emby";

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(2);

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
            LocalApiUrl = await ServerQuery.Instance.GetLocalApiUrlAsync();

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

        //Create Render Document Directives below
        private async Task<IDirective> RenderItemListSequenceTemplate(RenderDocumentTemplate template, IAlexaSession session)
        {
            ServerController.Instance.Log.Info("Render Document Started");

            var layout             = new List<IItem>();
            var touchWrapperLayout = new List<VisualItem>();
            var baseItems          = template.baseItems;
            var type               = baseItems[0].GetType().Name;

            ServerController.Instance.Log.Info($"Render Document is RenderItemListSequenceTemplate for {type}");

            baseItems.ForEach(async i => touchWrapperLayout.Add(await RenderItemPrimaryImageTouchWrapper(session, i, type)));

            ServerController.Instance.Log.Info($"Render Document has {baseItems.Count} Primary Image Touch Wrappers");

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
                        headerAttributionImage = template.HeaderAttributionImage != null ? $"{Url}{template.HeaderAttributionImage}" : ""
                    },
                    new Sequence()
                    {
                        height                 = "100vh",
                        width                  = "95vw",
                        top                    = "4vh",
                        left                   = "5vw",
                        scrollDirection        = "horizontal",
                        items                  = touchWrapperLayout
                    },
                    new AlexaFooter()
                    {
                        hintText               = "",
                        position               = "absolute",
                        bottom                 = "1vh",
                        id                     = "hint"
                    }
                }
            });

            ServerController.Instance.Log.Info("Render Document has Touch Wrapper Container");

            var sequenceItemsHintText = await GetSequenceItemsHintText(template.baseItems, session);

            ServerController.Instance.Log.Info("Render Document has Sequence Item Hint Texts");

            var scaleFadeInSequenceItems = new List<ICommand>();
            for (var i = 0; i < baseItems.Count; i++)
            {
                await semaphore.WaitAsync();
                scaleFadeInSequenceItems.Add(await Animations.ScaleFadeInItem(baseItems[i].InternalId.ToString(), 250, i*100));
                semaphore.Release();
            }

            ServerController.Instance.Log.Info("Render Document has primary image animations");

            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = "mediaItemSequence",
                document = new Document()
                {
                    theme   = "dark",
                    import  = Imports,
                    onMount = new List<ICommand>()
                    {
                        new Sequential()
                        {
                            commands = new List<ICommand>()
                            {
                                new Parallel()
                                {
                                    commands = scaleFadeInSequenceItems
                                },
                                new Sequential()
                                {
                                    commands    = sequenceItemsHintText.ToList(),
                                    repeatCount = 5
                                }
                            }
                        }
                    },
                    resources = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = layout
                    }
                }
            };

            ServerController.Instance.Log.Info("Render Document is creating view");

            return await Task.FromResult(view);
        }
        
        private async Task<IDirective> RenderItemDetailsTemplate(RenderDocumentTemplate template, IAlexaSession session)
        {
            ServerController.Instance.Log.Info("Render Document Started");

            var leftColumnSpacing = "36vw";

            var baseItem = template.baseItems[0];
            var type     = baseItem.GetType().Name;
            var item     = type.Equals("Season") ? baseItem.Parent : template.baseItems[0];
            
            var layout = new List<VisualItem>();
            const string token = "mediaItemDetails";

            ServerController.Instance.Log.Info($"Render Document is {token} for {item.Name}");
            
            (await GetVideoBackdropLayout(item, token)).ForEach(i => layout.Add(i));

            ServerController.Instance.Log.Info($"Render Document has {layout.Count} video backdrops");
            
            var graphicsDictionary = new Dictionary<string, Graphic>
            {
                {
                    "CheckMark", new Graphic()
                    {
                        height = 35,
                        width = 35,
                        viewportHeight = 25,
                        viewportWidth = 25,
                        items = new List<Path>()
                        {
                            new Path()
                            {
                                pathData = MaterialVectorIcons.CheckMark,
                                stroke = "none",
                                strokeWidth = "1px",
                                fill = template.baseItems[0].IsPlayed(session.User) ? "rgba(255,0,0,1)" : "white"
                            }
                        }
                    }
                },
                {
                    "Carousel", new Graphic()
                    {
                        height = 35,
                        width = 35,
                        viewportHeight = 25,
                        viewportWidth = 25,
                        items = new List<Path>()
                        {
                            new Path()
                            {
                                pathData =  MaterialVectorIcons.Carousel,
                                stroke = "none",
                                strokeWidth = "1px",
                                fill = "rgba(255,250,0,1)" 
                            }
                        }
                    }
                },
                {
                    "ArrayIcon", new Graphic()
                    {
                        height = 35,
                        width = 35,
                        viewportHeight = 25,
                        viewportWidth = 25,
                        items = new List<Path>()
                        {
                            new Path()
                            {
                                pathData =  MaterialVectorIcons.ArrayIcon,
                                stroke = "none",
                                strokeWidth = "1px",
                                fill = "rgba(255,250,0,1)" 
                            }
                        }
                    }
                }
            };

            var logo = item.HasImage(ImageType.Logo)
                ? $"{Url}/Items/{item.InternalId}/Images/logo?quality=90&maxHeight=508&maxWidth=200"
                : "";

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
                            await Animations.ScaleInOutOnPress(),
                            new SendEvent() {arguments = new List<object>() {"goBack"}}
                        }
                    }
                });
            }

            layout.Add(new Image()
            {
                id = "logo",
                source = logo,
                width = "12vw",
                //headerBackButton       = session.paging.canGoBack,
                position = "absolute",
                left = "85vw",
                //headerDivider          = true,
            });
            ServerController.Instance.Log.Info("Render Document has Header");

            //Name
            layout.Add(new Text()
            {
                text       = item.Name,
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
                text     = $"{(item.Genres.Any() ? item.Genres.Aggregate((genres, genre) => genres + ", " + genre) : "")}",
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
                text     = $"{item.ProductionYear} | {item.OfficialRating} | {GetRunTime(type, item)} | {GetEndTime(item)}",
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
                text = $"{item.Tagline}",
                style = "textStyleBody",
                left = leftColumnSpacing,
                top = "18vh",
                height = "10dp",
                width    = "40vw",
                fontSize = "22dp",
                id = "tag",
                display = !string.IsNullOrEmpty(item.Tagline) ? "normal" : "none",
                opacity = 0
            });
            ServerController.Instance.Log.Info("Render Document has Tag lines");

            //Watched check-mark
            layout.Add(new VectorGraphic()
            {
                source = "CheckMark",
                left = "87vw",
                position = "absolute",
                top = "30vh"
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
                id      = baseItem.InternalId.ToString(),
                onPress = new SendEvent() { arguments = new List<object>() { nameof(UserEventReadOverview) }},
                item    = new Container()
                {
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
                        new Text()
                        {
                            text     = $"{baseItem.Overview}",
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
                id = "primaryButton",
                position = "absolute",
                width = "38%",
                height = "75vh",
                top = "15vh",
                opacity = 1,
                items = new List<VisualItem>()
                {
                    new Image()
                    {
                        source = $"{Url}/Items/{baseItem.InternalId}/Images/primary?maxHeight=908&quality=90",
                        scale  = "best-fit",
                        height = "63vh",
                        width  = "100%",
                        id     = "primary"
                    },
                    // If we are playing here then we place a "Now Showing" icon instead of a button
                    //session.PlaybackStarted ?
                    //    new Frame()
                    //    {
                    //        backgroundColor = "red",
                    //        left = "2vw",
                    //        width = "33vw",
                    //        opacity = 0,
                    //        id = "showing",
                    //        paddingLeft = "7vw",
                    //        position = "absolute",
                    //        items = new List<IItem>()
                    //        {
                    //            new Text()
                    //            {
                    //                text = "NOW PLAYING",
                    //                fontSize = "35dp"
                    //            }
                    //        }
                    //    } :
                        // ReSharper disable once ComplexConditionExpression
                         await GetButtonFrame(args : type == "Movie" || type == "Episode" 
                                ? new List<object>() { nameof(UserEventPlaybackStart), session.room != null ? session.room.Name : "" } 
                                : new List<object>() { nameof(UserEventShowItemListSequenceTemplate) },
                                   icon : item.GetType().Name == "Series" ?  MaterialVectorIcons.ListIcon : MaterialVectorIcons.PlayOutlineIcon,
                                   id   : template.baseItems[0].InternalId.ToString())
                }
            });
            ServerController.Instance.Log.Info("Render Document has Primary Image");

            //if (session.PlaybackStarted)
            //{
            //    layout.Add(new AlexaProgressBar()
            //    {
            //        id = "playbackProgress",
            //        position = "absolute",
            //        bottom = "15vh",
            //        width = "50vw",
            //        left = "45vw",
            //        progressBarType = ProgressBarType.determinate,
            //        progressValue = 0.0,
            //        totalValue = 100.0,

            //    });
                
            //}

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
                                        source       = $"{Url}/Items/{template.baseItems[0].InternalId}/Images/backdrop?maxWidth=1200&amp;maxHeight=800&amp;quality=90",
                                        scale        = "best-fill",
                                        width        = "100vw",
                                        height       = "100vh",
                                        position     = "absolute",
                                        overlayColor = "rgba(0,0,0,0.55)"
                                    },
                                    new Image()
                                    {
                                        source = $"{Url}/Items/{template.baseItems[0].InternalId}/Images/logo?quality=90",
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
                                width  = "100vw",
                                height = "100vh",
                                items  = layout,
                            }
                        }
                    },
                    //handleTick = new List<HandleTick>()
                    //{
                    //    new HandleTick() {
                    //        minimumDelay = 1000,
                    //        commands = new List<ICommand>()
                    //        {
                    //            new SendEvent()
                    //            {
                    //                arguments = new List<object>() { nameof(PlaybackProgressValueUpdate), token}
                    //            }
                    //        }
                    //    }
                    //}
                }
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
                                    await GetButtonFrame(new List<object>() {"ShowVerticalTextListTemplate"},  MaterialVectorIcons.Right, "ScrollNext" )
                                }
                            },
                            new Container()
                            {
                                position = "absolute",
                                items = new List<VisualItem>()
                                {
                                    await GetButtonFrame(new List<object>() {"ShowVerticalTextListTemplate"},  MaterialVectorIcons.Left, "ScrollPrev" )
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
            
            var imageEndpoint  = $"/Items/{template.baseItems[0].InternalId}/Images";
            var layout         = new List<VisualItem>();
            const string token = "roomSelection";

            (await GetVideoBackdropLayout(template.baseItems[0], token)).ForEach(b => layout.Add(b));
            
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
                source = Url + imageEndpoint + "/logo?quality=90",
                width = "25vw",
                height = "10vh",
                right = "5vw",
                bottom = "5vh"
            });

            var roomButtons = RenderRoomButtonLayout(template);
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
            var layout = new List<VisualItem>();
            const string token = "browseLibrary";

            layout.Add(new AlexaBackground()
            {
                backgroundVideoSource = new List<Source>()
                {
                    new Source()
                    {
                        url         = $"{Url}/MoviesLibrary",
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

            layout.Add(new Video()
            {
                source = new List<Source>()
                {
                    new Source()
                    {
                        url         = $"{Url}/Question",
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
                source = $"{Url}/EmptyPng?quality=90",
                opacity = 0.45
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

            layout.Add(new Video()
            {
                source = new List<Source>()
                {
                    new Source()
                    {
                        url         = $"{Url}/particles",
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
                source = $"{Url}/EmptyPng?quality=90",
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

            layout.Add(new Video()
            {
                source = new List<Source>()
                {
                    new Source()
                    {
                        url         = $"{Url}/particles",
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
                source = $"{Url}/EmptyPng?quality=90",
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
            
            RenderAudioBuilder.HelpStrings.ForEach(s => helpItems.Add(new Text()
            {
                id                = "helpText",
                text              = s,
                textAlign         = "center",
                textAlignVertical = "center",
                paddingRight      = "20dp",
                paddingLeft       = "20dp"
            }));

            var view = new Directive()
            {
                type = "Alexa.Presentation.APL.RenderDocument",
                token = "Help",
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
                                items  = new List<VisualItem>()
                                {
                                    new AlexaBackground()
                                    {
                                        backgroundVideoSource = new List<Source>()
                                        {
                                            new Source()
                                            {
                                                repeatCount = 15,
                                                url         = $"{Url}/MovingFloor"
                                            }
                                        },
                                        backgroundScale = "best-fill",
                                        videoAudioTrack = "none",
                                        videoAutoPlay   = true,
                                        overlayGradient = true
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


        //Create components below
        private static async Task<Frame> GetButtonFrame(List<object> args, string icon, string id = "")
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

        private static async Task<TouchWrapper> RenderItemPrimaryImageTouchWrapper(IAlexaSession session, BaseItem item, string type)
        {
            var IsMovie    = type.Equals("Movie");
            var IsTrailer  = type.Equals("Trailer");
            var IsEpisode  = type.Equals("Episode");

            return await Task.FromResult(new TouchWrapper()
            {
                id = item.InternalId.ToString(),
                opacity = 1,
                onPress = new Parallel()
                {
                    commands = new List<ICommand>()
                    {
                        await Animations.ScaleInOutOnPress(),
                        new SendEvent()
                        {
                            arguments = IsMovie || IsTrailer ? new List<object>() { nameof(UserEventShowBaseItemDetailsTemplate) }
                                : IsEpisode ? new List<object>() { nameof(UserEventPlaybackStart), session.room != null ? session.room.Name : ""}
                                : new List<object>() { nameof(UserEventShowItemListSequenceTemplate) }
                        }
                    }
                },
                items = new List<VisualItem>()
                {
                    type == "Episode" ? await RenderEpisodePrimaryImageContainer(item) : await RenderMoviePrimaryImageContainer(item)
                }
                    
            });
        }

        private static async Task<Container> RenderMoviePrimaryImageContainer(BaseItem item)
        {
            return await Task.FromResult(new Container()
            {
                items = new List<VisualItem>()
                {
                    new Image()
                    {
                        source       = $"{Url}/Items/{item.InternalId}/Images/primary?quality=90&amp;maxHeight=1008&amp;maxWidth=700&amp;",
                        width        = "30vw",
                        height       = "62vh",
                        paddingRight = "12px",
                    }
                }
            });
        }

        private static async Task<Container> RenderEpisodePrimaryImageContainer(BaseItem item)
        {
            var primaryId = 0L;
            var imageType = string.Empty;	
            switch (item.HasImage(ImageType.Primary))
            {
                case true:
                    primaryId = item.InternalId;
                    imageType = "primary";
                    break;

                case false:
                    primaryId = item.Parent.Parent.InternalId;
                    imageType = "backdrop/0";
                    break;
            } 

            return await Task.FromResult(new Container()
            {
                height = "70vh",
                width  = "30vw",
                items  = new List<VisualItem>()
                {
                    new Image()
                    {
                        source       = $"{Url}/Items/{primaryId}/Images/{imageType}?quality=90&amp;maxHeight=508&amp;maxWidth=600&amp;",
                        width        = "30vw",
                        height       = "62vh",
                        paddingRight = "12px",
                    },
                    new Text()
                    {
                        text         = $"{item.Name}",
                        style        = "textStyleBody",
                        top          = "-15vh",
                        fontSize     = "20dp"
                    },
                    new Text()
                    {
                        text         = $"Episode {item.IndexNumber}",
                        style        = "textStyleBody",
                        top          = "-15vh",
                        fontSize     = "20dp"
                    },
                    new Text()
                    {
                        text = item.PremiereDate?.ToString("D"),
                        style        = "textStyleBody",
                        top          = "-15vh",
                        fontSize     = "15dp"
                    }
                }
            });
        }

        private List<VisualItem> RenderRoomButtonLayout(RenderDocumentTemplate template)
        {
            var config = Plugin.Instance.Configuration;
            var roomButtons = new List<VisualItem>();

            if (config.Rooms is null) return roomButtons;

            System.Threading.Tasks.Parallel.ForEach(config.Rooms, async room =>
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
        
        private async Task<List<VisualItem>> GetVideoBackdropLayout(BaseItem baseItem, string token)
        {
            var videoBackdropIds = baseItem.ThemeVideoIds;
            // ReSharper disable once TooManyChainedReferences
            var videoBackdropId = videoBackdropIds.Length > 0 ? ServerQuery.Instance.GetItemById(videoBackdropIds[0]).InternalId.ToString() : string.Empty;
            
            var backdropImageUrl     = $"{Url}/Items/{baseItem.InternalId}/Images/backdrop?maxWidth=1200&amp;maxHeight=800&amp;quality=90";
            var videoBackdropUrl     = $"{Url}/videos/{videoBackdropId}/stream.mp4";
            var videoBackdropOverlay = $"{Url}/EmptyPng?quality=90";

           
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
                                url         = $"{videoBackdropUrl}",
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
                        onEnd = new List<object>()
                        {
                            new SendEvent()
                            {
                                delay     = 1200, //We have to delay so Alexa can speak
                                arguments = new List<object>() { nameof(VideoOnEnd), token, backdropImageUrl }
                            },
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

        private static string GetRunTime(string type, BaseItem baseItem)
        {
            if (!string.Equals(type, "Movie")) return string.Empty;
            var runTimeTicks = baseItem.RunTimeTicks;
            return !(runTimeTicks is null) ? $"{TimeSpan.FromTicks(runTimeTicks.Value).TotalMinutes.ToString(CultureInfo.InvariantCulture).Split('.')[0]} minutes" : string.Empty;
        }

        private static string GetEndTime(BaseItem baseItem)
        {
            return
                $"Ends at: {DateTime.Now.AddTicks(baseItem.GetRunTimeTicksForPlayState()).ToString("h:mm tt", CultureInfo.InvariantCulture)}";
        }

        private static async Task<IEnumerable<ICommand>> GetSequenceItemsHintText(IList<BaseItem> sequenceItems, IAlexaSession session)
        {
            if (session.PlaybackStarted) return new List<Command>();

            var type = sequenceItems[0]?.GetType().Name;
            return await Task.FromResult((sequenceItems.Count >= 3 ? sequenceItems.Take(3) : sequenceItems).ToList()
                .SelectMany(item => new List<ICommand>()
                {
                    new AnimateItem()
                    {
                        componentId = "hint",
                        duration    = 1020,
                        easing      = "ease-in",
                        value       = new List<IValue>()
                        {
                            new OpacityValue() {@from = 1, to = 0}
                        },
                        delay = 5000
                    },
                    new SetValue()
                    {
                        componentId = "hint",
                        property    = "hintText",
                        value       = type == "Episode" ? $"Try \"Alexa, play episode {item.IndexNumber}\""
                                    : type == "Season" ? $"Try \"Alexa, show season {item.IndexNumber}\""
                                    : $"Try \"Alexa, show the {type?.ToLowerInvariant()} {item.Name}\"" //Default "Series"/"Movie"
                    },
                    new AnimateItem()
                    {
                        componentId = "hint",
                        duration    = 1020,
                        easing      = "ease-out",
                        value       = new List<IValue>()
                        {
                            new OpacityValue() {@from = 0, to = 1}
                        },
                        delay = 2500
                    }
                }));

            
        }
        
    }
}