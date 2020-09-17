using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.Components;
using AlexaController.Alexa.Presentation.APL.VectorGraphics;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using Parallel = AlexaController.Alexa.Presentation.APL.Commands.Parallel;
using Source = AlexaController.Alexa.Presentation.APL.Components.Source;
using Video = AlexaController.Alexa.Presentation.APL.Components.Video;

// ReSharper disable twice TooManyChainedReferences
// ReSharper disable twice InconsistentNaming
// ReSharper disable twice ComplexConditionExpression


/*
 * Echo Display Devices want the **internal LAN address** for Images when using the skill on the same network as emby server (ex. 192.168.X.XXX:8096)
 * We do not need to serve media requests over https like the documentation implies, Weird!
 * Is it the reverse proxy handling this conversion?
 * Is it because the echo shows are on the same network as the server?
 * TODO: Find out why this is happening
 */

namespace AlexaController
{
    public enum RenderDocumentType
    {
        NONE,
        ITEM_DETAILS_TEMPLATE,
        ITEM_LIST_SEQUENCE_TEMPLATE,
        BROWSE_LIBRARY_TEMPLATE,
        QUESTION_TEMPLATE,
        VIDEO,
        NOT_UNDERSTOOD,
        HELP,
        GENERIC_HEADLINE_TEMPLATE,
        ROOM_SELECTION_TEMPLATE,
        VERTICAL_TEXT_LIST_TEMPLATE
    }

    public interface IRenderDocumentTemplate
    {
        RenderDocumentType renderDocumentType { get; }
        string HeaderTitle                    { get; }
        List<BaseItem> baseItems              { get; }
        BaseItem collectionRoot               { get; }
        string HeadlinePrimaryText            { get; }
    }

    public class RenderDocumentTemplate : IRenderDocumentTemplate
    {                                                                 
        public RenderDocumentType renderDocumentType { get; set; } 
        public string HeaderTitle                    { get; set; } = "";
        public List<BaseItem> baseItems              { get; set; }
        public BaseItem collectionRoot               { get; set; }
        public string HeadlinePrimaryText            { get; set; } = "";
    }

    public class RenderDocumentBuilder : IServerEntryPoint
    {
        private ILibraryManager LibraryManager       { get; }
        private ISessionManager SessionManager       { get; }
        private IServerApplicationHost Host          { get; }
        public static RenderDocumentBuilder Instance { get; private set; }
        private static string LanAddress             { get; set; }

        private readonly List<IImport> Imports = new List<IImport>()
        {
            new Import()
            {
                name                          = "alexa-layouts",
                version                       = "1.1.0"
            },
            new Import()
            {
                name                          = "alexa-viewport-profiles",
                version                       = "1.1.0"
            }
        };

        private readonly List<IResource> Resources = new List<IResource>()
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

        private static string CastIcon        => "M1,10V12A9,9 0 0,1 10,21H12C12,14.92 7.07,10 1,10M1,14V16A5,5 0 0,1 6,21H8A7,7 0 0,0 1,14M1,18V21H4A3,3 0 0,0 1,18M21,3H3C1.89,3 1,3.89 1,5V8H3V5H21V19H14V21H21A2,2 0 0,0 23,19V5C23,3.89 22.1,3 21,3Z";
        private static string EmbyIcon        => "M11,2L6,7L7,8L2,13L7,18L8,17L13,22L18,17L17,16L22,11L17,6L16,7L11,2M10,8.5L16,12L10,15.5V8.5Z";
        private static string PlayOutlineIcon => "M8.5,8.64L13.77,12L8.5,15.36V8.64M6.5,5V19L17.5,12";
        private static string CheckMark       => "M21,7L9,19L3.5,13.5L4.91,12.09L9,16.17L19.59,5.59L21,7Z";
        private static string Url             => $"{LanAddress}/emby";
        private static string More_Horizontal => "M6 10c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm12 0c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm-6 0c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z";
        private static string ListIcon        => "M3 13h2v-2H3v2zm0 4h2v-2H3v2zm0-8h2V7H3v2zm4 4h14v-2H7v2zm0 4h14v-2H7v2zM7 7v2h14V7H7z";
        private static string Left            => "M15.41 16.59L10.83 12l4.58-4.59L14 6l-6 6 6 6 1.41-1.41z";
        private static string Right           => "M8.59 16.59L13.17 12 8.59 7.41 10 6l6 6-6 6-1.41-1.41z";

        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(2);

        public RenderDocumentBuilder(ILibraryManager libraryManager, ISessionManager sessionManager, IServerApplicationHost host)
        {
            LibraryManager = libraryManager;
            SessionManager = sessionManager;
            Host           = host;
            LanAddress     = Host.GetSystemInfo(CancellationToken.None).Result.LocalAddress;
            Instance       = this;
        }

        public IDirective GetRenderDocumentTemplate(IRenderDocumentTemplate template, IAlexaSession session)
        {
            switch (template.renderDocumentType)
            {
                case RenderDocumentType.BROWSE_LIBRARY_TEMPLATE     : return GetBrowseLibraryTemplate(template, session);
                case RenderDocumentType.ITEM_DETAILS_TEMPLATE       : return GetItemDetailsTemplate(template, session);
                case RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE : return GetItemListSequenceTemplate(template, session);
                case RenderDocumentType.QUESTION_TEMPLATE           : return GetQuestionRequestTemplate(template);
                case RenderDocumentType.ROOM_SELECTION_TEMPLATE     : return GetRoomSelectionTemplate(template, session);
                case RenderDocumentType.VIDEO                       : return GetRenderVideo(template);
                case RenderDocumentType.NOT_UNDERSTOOD              : return GetNotUnderstoodRequestTemplate();
                case RenderDocumentType.VERTICAL_TEXT_LIST_TEMPLATE : return GetVerticalTextListTemplate(template, session);
                case RenderDocumentType.HELP                        : return GetHelpTemplate();
                case RenderDocumentType.GENERIC_HEADLINE_TEMPLATE   : return GetGenericHeadlineRequestTemplate(template);
                case RenderDocumentType.NONE                        : return null;
                default                                             : return null;
            }
        }

        private IDirective GetItemListSequenceTemplate(IRenderDocumentTemplate template, IAlexaSession session, string scrollDirection = "horizontal")
        {

            var layout             = new List<IItem>();
            var touchWrapperLayout = new List<IItem>();
            var baseItems          = template.baseItems;
            var type               = baseItems[0].GetType().Name;

            var collectionRoot = type == "Season" ? baseItems[0].Parent : type == "Episode" ? baseItems[0].Parent.Parent : template.collectionRoot;
            
            // Wrap each media items Primary Image in a touch element for display
            baseItems.ForEach(i => touchWrapperLayout.Add(new TouchWrapper()
            {
                id      = i.InternalId.ToString(),
                opacity = 0,
                onPress = new Parallel()
                {
                    commands = new List<ICommand>()
                    {
                        new Sequential()
                        {
                            commands = new List<ICommand>()
                            {
                                new AnimateItem()
                                {
                                    easing      = "ease",
                                    duration    = 250,
                                    value = new List<IValue>()
                                    {
                                        new TransitionValue()
                                        {
                                            from = new List<From>()
                                            {
                                                new From() { scaleX = 1, scaleY = 1 }
                                            },
                                            to = new List<To>()
                                            {
                                                new To() { scaleX = 0.9, scaleY = 0.9 }
                                            }
                                        }
                                    }
                                },
                                new AnimateItem()
                                {
                                    easing      = "ease",
                                    duration    = 250,
                                    value = new List<IValue>()
                                    {
                                        new TransitionValue()
                                        {
                                            from = new List<From>()
                                            {
                                                new From() { scaleX = 0.9, scaleY = 0.9 }
                                            },
                                            to = new List<To>()
                                            {
                                                new To() { scaleX = 1, scaleY = 1 }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new SendEvent()
                        {
                            arguments = type == "Movie" ? new List<object>() { "UserEventShowBaseItemDetailsTemplate" }
                                : type == "Episode" ? new List<object>() { "UserEventPlaybackStart", session.room != null ? session.room.Name : ""}
                                : new List<object>() { "UserEventShowItemListSequenceTemplate" }
                        }
                    }
                },

                // Item types in the list are Episodes, place episode info text under each image
                items = type == "Episode" ?
                    new List<IItem>()
                    {
                        new Container()
                        {
                            height = "70vh",
                            width  = "30vw",
                            items  = new List<IItem>()
                            {
                                new Image()
                                {
                                    source       = $"{Url}/Items/{i.InternalId}/Images/primary?quality=90&amp;maxHeight=1008&amp;maxWidth=700&amp;",
                                    width        = "30vw",
                                    height       = scrollDirection == "horizontal" ? "62vh" : "25vh",
                                    paddingRight = "12px",
                                },
                                new Text()
                                {
                                    text         = $"Episode {i.IndexNumber}",
                                    style        = "textStyleBody",
                                    top          = "-15vh",
                                    fontSize     = "20dp"
                                },
                                new Text()
                                {
                                    text         = $"{i.Name}",
                                    style        = "textStyleBody",
                                    top          = "-15vh",
                                    fontSize     = "20dp"
                                }
                            }
                        }
                    } : // Not an "Episode" - no need to place text under the image, just the primary image
                    new List<IItem>()
                    {
                        new Image()
                        {
                            source       = $"{Url}/Items/{i.InternalId}/Images/primary?quality=90&amp;maxHeight=1008&amp;maxWidth=700&amp;",
                            width        = "30vw",
                            height       = scrollDirection == "horizontal" ? "62vh" : "25vh",
                            paddingRight = "12px",
                        }
                    }
            }));
            
            layout.Add(new Container()
            {
                id     = "primary",
                width  = "100vw",
                height = "100vh",
                items  = new List<IItem>()
                {
                    new AlexaHeader()
                    {
                        headerTitle            = $"{template.HeaderTitle}",
                        headerBackButton       = session.paging.canGoBack,
                        headerDivider          = true,
                        headerAttributionImage = !(collectionRoot is null) ? collectionRoot.HasImage(ImageType.Logo) ? $"{Url}/Items/{collectionRoot.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : "" : "",
                    },
                    new Sequence()
                    {
                        height                 = "100vh",
                        width                  = "95vw",
                        top                    = "4vh",
                        left                   = "5vw",
                        scrollDirection        = scrollDirection,
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

           
           /*
             If we load these animations to slowly and with too much of a delay
             the document doesn't have enough time to load larger collection in a sequence,
             items are then missed at the end
             It's a limitation of the Echo Show devices
             use a semaphore slim to limit the data in the loop
            */
            var scaleFadeInSequenceItems = new List<ICommand>();
            for (var i = 0; i < baseItems.Count; i++)
            {
                semaphore.Wait();
                scaleFadeInSequenceItems.Add(ScaleFadeInItem(baseItems[i].InternalId.ToString(), 200, i*100));
                semaphore.Release();
            }

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
                                    commands    = GetSequentialItemsHintText(template.baseItems, session).ToList(),
                                    repeatCount = 5
                                }
                            }
                            
                        }
                    },
                    resources    = Resources,
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = layout
                    }
                }
            };

            
            return view;
        }

        private IDirective GetItemDetailsTemplate(IRenderDocumentTemplate template, IAlexaSession session)
        {
            
            var type = template.baseItems[0].GetType().Name;

            var baseItem = type.Equals("Season") ? template.baseItems[0].Parent : template.baseItems[0];
            
            var layout = new List<IItem>();
            const string token = "mediaItemDetails";

            GetVideoBackdropLayout(baseItem, token).ForEach(i => layout.Add(i));

            layout.Add(new AlexaHeader()
            {
                headerTitle            = template.HeaderTitle != "" ? template.HeaderTitle : template.baseItems[0].Name,
                headerAttributionImage = template.baseItems[0].HasImage(ImageType.Logo) ? $"{Url}/Items/{baseItem.InternalId}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : "",
                headerBackButton       = session.paging.canGoBack,
                headerDivider          = true,
            });
            //Room - Rating
            layout.Add(new Text()
            {
                text = $"{(session.room != null ? session.room.Name.ToUpperInvariant() : string.Empty)} | Rated {baseItem.OfficialRating}",
                style = "textStyleBody",
                left = "42%",
                top = "3vh"
            });
            //Genres
            layout.Add(new Text()
            {
                // ReSharper disable once TooManyChainedReferences
                text = $"{(baseItem.Genres.Any() ? baseItem.Genres.Aggregate((i, j) => i + ", " + j) : "")}",
                left = "42%",
                top = "6vh",
                width = "40vw",
                height = "12dp",
                fontSize = "18dp",
                id = "genre"
            });
            //TagLines
            layout.Add(new Text()
            {
                text = $"<b>{baseItem.Tagline}</b>",
                style = "textStyleBody",
                left = "42vw",
                top = "12vh",
                height = "10dp",
                width = "35vw",
                fontSize = "22dp",
                id = "tag",
                display = !string.IsNullOrEmpty(baseItem.Tagline) ? "normal" : "none"
            });
            //Watched check-mark
            layout.Add(new VectorGraphic()
            {
                source = "CheckMark",
                left = "87vw"
            });
            //Runtime span
            var runTimeTicks = template.baseItems[0].RunTimeTicks;
            if (!ReferenceEquals(null, runTimeTicks))
                layout.Add(new Text()
                {
                    text = $"{TimeSpan.FromTicks(runTimeTicks.Value).TotalMinutes.ToString(CultureInfo.InvariantCulture).Split('.')[0]} minutes",
                    style = "textStyleBody",
                    left = "82%",
                    top = "3vh",
                    width = "15vw",
                    height = "5%",
                    fontSize = "18dp",
                    id = "timespan"
                });
            //End Time
            layout.Add(new Text()
            {
                text     = $"Ends at: {DateTime.Now.AddTicks(template.baseItems[0].GetRunTimeTicksForPlayState()).ToString("h:mm tt", CultureInfo.InvariantCulture)}",
                style    = "textStyleBody",
                left     = "82%",
                top      = "4vh",
                width    = "55vw",
                height   = "5%",
                fontSize = "18dp",
                id       = "endTime"
            });
            //Overview
            layout.Add(new ScrollView()
            {
                top     = "9vh",
                left    = "42vw",
                id      = "overview",
                height  = "25vh",
                opacity = 0,
                item    = new Text()
                {
                    text = $"{template.baseItems[0].Overview}",
                    style = "textStyleBody",
                    width = "55vw",
                    fontSize = "20dp"
                }
            });
            //Primary Image
            layout.Add(new Container()
            {
                id = "playButton",
                position = "absolute",
                width = "38%",
                height = "75vh",
                left = "3%",
                top = "20%",
                opacity = 1,
                items = new List<IItem>()
                {
                    new Image()
                    {
                        source = $"{Url}/Items/{template.baseItems[0].InternalId}/Images/primary?maxWidth=400&amp;maxHeight=708&amp;quality=90",
                        scale  = "best-fit",
                        height = "63vh",
                        width  = "100%",
                        id     = "primary"
                    },
                    // If we are playing here then we place a "Now Showing" icon instead of a button
                    session.PlaybackStarted ?
                        new Frame()
                        {
                            backgroundColor = "red",
                            left = "2vw",
                            width = "33vw",
                            opacity = 0,
                            id = "showing",
                            paddingLeft = "7vw",
                            position = "absolute",
                            items = new List<IItem>()
                            {
                                new Text()
                                {
                                    text = "NOW PLAYING",
                                    fontSize = "35dp"
                                }
                            }
                        }
                        : GetButtonFrame(args : type == "Movie" || type == "Episode" 
                                ? new List<object>() { "UserEventPlaybackStart", session.room != null ? session.room.Name : "" } 
                                : new List<object>() { "UserEventShowItemListSequenceTemplate" },
                                   icon : baseItem.GetType().Name == "Series" ? ListIcon : PlayOutlineIcon,
                                   id   : template.baseItems[0].InternalId.ToString())
                }
            });

            layout.Add(new AlexaFooter()
            {
                hintText = "Alexa, play that...",
                position = "absolute",
                bottom = "1vh"
            });

            var view = new Directive()
            {
                type     = "Alexa.Presentation.APL.RenderDocument",
                token    = token,
                document = new Document()
                {
                    theme    = "dark",
                    settings = new Settings() { idleTimeout = 120000 },
                    import   = Imports,
                    graphics = new Dictionary<string, Graphic>()
                    {
                        {
                            "CheckMark", new Graphic()
                            {
                                height         = 35,
                                width          = 35,
                                viewportHeight = 25,
                                viewportWidth  = 25,
                                items          = new List<IItem>()
                                {
                                    new Path()
                                    {
                                        pathData    = CheckMark,
                                        stroke      = "none",
                                        strokeWidth = "1px",
                                        fill        = template.baseItems[0].IsPlayed(session.User) ? "rgba(255,0,0,1)" : "white"
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
                                ScaleFadeInItem("playButton", 800),
                                FadeInItem("overview", 800),
                                FadeInItem("showing", 2000)
                            }
                        }
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
                                items  = new List<IItem>()
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
                                items  = layout
                            }
                        }
                    }
                }
            };

            
            return view;
        }

        private IDirective GetVerticalTextListTemplate(IRenderDocumentTemplate template, IAlexaSession session)
        {
            var layout = new List<IItem>();
            var layoutBaseItems = new List<IItem>();
            var baseItems = template.baseItems;

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
                            arguments =  new List<object>() { "UserEventShowBaseItemDetailsTemplate" }
                        }
                    }
                },
                items = new List<IItem>()
                {
                    new Container()
                    {
                        direction   = "row",
                        paddingLeft = "12vw",
                        paddingTop  = "4vh",
                        items = new List<IItem>()
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
                                items = new List<IItem>()
                                {
                                    GetButtonFrame(new List<object>() {"ShowVerticalTextListTemplate"}, Right, "ScrollNext" )
                                }
                            },
                            new Container()
                            {
                                position = "absolute",
                                items = new List<IItem>()
                                {
                                    GetButtonFrame(new List<object>() {"ShowVerticalTextListTemplate"}, Left, "ScrollPrev" )
                                }
                            },
                        }
                    }
                }
            };

            return view;
        }

        private Frame GetButtonFrame(List<object> args, string icon, string id = "")
        {

            var buttonPressAnimation = new Sequential()
            {
                commands = new List<ICommand>()
                {
                    new AnimateItem()
                    {
                        easing = "ease",
                        duration = 250,
                        value = new List<IValue>()
                        {
                            new TransitionValue()
                            {
                                @from = new List<From>() { new From() { scaleX = 1, scaleY  = 1 } },
                                to    = new List<To>()   { new To()   { scaleX = 0.9, scaleY = 0.9} }
                            }
                        }
                    },
                    new AnimateItem()
                    {
                        easing = "ease",
                        duration = 250,
                        value = new List<IValue>()
                        {
                            new TransitionValue()
                            {
                                @from = new List<From>() { new From() { scaleX = 0.9, scaleY = 0.9 } },
                                to = new List<To>()      { new To()   { scaleX = 1,   scaleY = 1 } }
                            }
                        }
                    }
                }
            };

            return new Frame()
            {
                position        = "absolute",
                top             = "29vh",
                right           = "40%",
                borderWidth     = "3px",
                borderColor     = "white",
                borderRadius    = "75px",
                backgroundColor = "rgba(0,0,0,0.35)",
                items = new List<IItem>()
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
                                buttonPressAnimation,
                                new SendEvent() {arguments = args}
                            }
                        }
                    }
                }
            };
        }

        private IDirective GetRoomSelectionTemplate(IRenderDocumentTemplate template, IAlexaSession session)
        {
            var endpoint = $"/Items/{template.baseItems[0].InternalId}/Images";
            var layout = new List<IItem>();
            const string token = "roomSelection";

            GetVideoBackdropLayout(template.baseItems[0], token).ForEach(b => layout.Add(b));

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
                source = endpoint + "/logo?quality=90",
                width = "25vw",
                height = "10vh",
                right = "5vw",
                bottom = "5vh"
            });

            GetRoomButtonLayout(template).ForEach(b => layout.Add(b));

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

            //AlexaSessionManager.Instance.UpdateSessionRenderDocumentPages(session, templateInfo);
            return view;
        }

        private IDirective GetBrowseLibraryTemplate(IRenderDocumentTemplate template, IAlexaSession session)
        {
            var layout = new List<IItem>();
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
                    //graphics     = new Dictionary<string, Graphic>()
                    //{
                    //    {
                    //        "EmbyIcon", new Graphic()
                    //        {
                    //            height         = 90,
                    //            width          = 90,
                    //            viewportHeight = 25,
                    //            viewportWidth  = 25,
                    //            items = new List<Item>()
                    //            {
                    //                new Path()
                    //                {
                    //                    pathData    = EmbyIcon,
                    //                    stroke      = "none",
                    //                    strokeWidth = "1px",
                    //                    fill        = "rgba(255,255,255,0.3)"
                    //                }
                    //            }
                    //        }
                    //    }
                    //},
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

            return view;
        }

        private IDirective GetQuestionRequestTemplate(IRenderDocumentTemplate template)
        {
            var layout = new List<IItem>();

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

            return view;
        }

        private IDirective GetNotUnderstoodRequestTemplate()
        {
            var layout = new List<IItem>();

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

            return view;
        }

        private IDirective GetGenericHeadlineRequestTemplate(IRenderDocumentTemplate template)
        {
            var layout = new List<IItem>();

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

            return view;
        }
        
        private IDirective GetRenderVideo(IRenderDocumentTemplate template)
        {
            //Not currently used
            var videoUrl = "https://theater.unityhome.online/emby/videos/stream.mp4";

            var view = new VideoApp()
            {
                videoItem = new VideoItem()
                {
                    source = $"{videoUrl}",
                    metadata = new Metadata()
                    {
                        title = template.baseItems[0].Name
                    }
                }
            };
            return view;
        }
        
        private IDirective GetHelpTemplate()
        {
            var helpItems = new List<IItem>();
            
            SemanticSpeechStrings.HelpStrings.ForEach(s => helpItems.Add(new Text()
            {
                text = s,
                textAlign = "center",
                textAlignVertical = "center",
                paddingRight = "20dp",
                paddingLeft = "20dp"
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
                                items  = new List<IItem>()
                                {
                                    new AlexaBackground()
                                    {
                                        backgroundVideoSource = new List<Source>()
                                        {
                                            new Source()
                                            {
                                                repeatCount = 10,
                                                url         = $"{Url}/particles"
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
                                            new SendEvent() { arguments = new List<object>() {"HelpPager", "Help", "${event.source.value}" } }
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
            };

            return view;
        }



        //On the room selection screen show the user the configured room, and mark them as available or not.
        private List<IItem> GetRoomButtonLayout(IRenderDocumentTemplate template)
        {
            var config = Plugin.Instance.Configuration;
            var roomButtons = new List<IItem>();

            if (config.Rooms is null) return roomButtons;

            foreach (var room in config.Rooms)
            {
                var disabled = true;

                foreach (var session in SessionManager.Sessions)
                {
                    if (session.DeviceName == room.Device) disabled = false;
                    if (session.Client     == room.Device) disabled = false;
                }

                roomButtons.Add(new Container()
                {
                    direction = "row",
                    left = "15vw",
                    top = "10vh",
                    items = new List<IItem>()
                    {
                        new AlexaIconButton()
                        {
                            id            = template.baseItems[0].InternalId.ToString(),
                            buttonSize    = "72dp",
                            vectorSource  = CastIcon,
                            disabled      = disabled,
                            primaryAction = new Sequential()
                            {
                                commands = new List<ICommand>()
                                {
                                    new AnimateItem()
                                    {
                                        easing      = "ease",
                                        duration    = 250,
                                        value = new List<IValue>()
                                        {
                                            new TransitionValue()
                                            {
                                                from = new List<From>()
                                                {
                                                    new From() { scaleX = 1, scaleY = 1 }
                                                },
                                                to = new List<To>()
                                                {
                                                    new To() { scaleX = 0.9, scaleY = 0.9 }
                                                }
                                            }
                                        }
                                    },
                                    new AnimateItem()
                                    {
                                        easing      = "ease",
                                        duration    = 250,
                                        value = new List<IValue>()
                                        {
                                            new TransitionValue()
                                            {
                                                @from = new List<From>()
                                                {
                                                    new From() { scaleX = 0.9, scaleY = 0.9 }
                                                },
                                                to = new List<To>()
                                                {
                                                    new To() { scaleX = 1, scaleY = 1 }
                                                }
                                            }
                                        }
                                    },
                                    new SendEvent() { arguments = new List<object>() { "UserEventPlaybackStart", room.Name } },
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
            }

            return roomButtons;
        }

        //Use a video backdrop layout if available, then switch to a static image for the media item.
        //If no video backdrop is  available show a static background image.
        private List<IItem> GetVideoBackdropLayout(BaseItem baseItem, string token)
        {
            var videoBackdropIds = baseItem.ThemeVideoIds;
            var videoBackdropId = videoBackdropIds.Length > 0 ? LibraryManager.GetItemById(videoBackdropIds[0]).InternalId.ToString() : string.Empty;


            var backdropImageUrl = $"{Url}/Items/{baseItem.InternalId}/Images/backdrop?maxWidth=1200&amp;maxHeight=800&amp;quality=90";
            var videoBackdropUrl = $"{Url}/videos/{videoBackdropId}/stream.mp4";
            var videoBackdropOverlay = $"{Url}/EmptyPng?quality=90";

            var layout = new List<IItem>();
            if (!string.IsNullOrEmpty(videoBackdropId))
            {
                layout.Add(new Video()
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
                            arguments = new List<object>() { "VideoOnEnd", token, backdropImageUrl }
                        },
                    }

                });
                layout.Add(new Image()
                {
                    overlayColor = "rgba(0,0,0,1)",
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    source = videoBackdropOverlay,
                    opacity = 0.65,
                    id = "backdropOverlay"
                });
            }
            else
            {
                layout.Add(new Image()
                {
                    overlayColor = "rgba(0,0,0,0.55)",
                    scale = "best-fill",
                    width = "100vw",
                    height = "100vh",
                    position = "absolute",
                    source = backdropImageUrl
                });
            }

            return layout;
        }

        //Create text at the bottom of the echo show screen which will give hints to the user for what they can say.
        //Use the currently displayed media items in the hint text.
        private IEnumerable<ICommand> GetSequentialItemsHintText(IList<BaseItem> sequenceItems, IAlexaSession session)
        {
            if (session.PlaybackStarted) return new List<Command>();

            var type = sequenceItems[0]?.GetType().Name;
            return (sequenceItems.Count >= 3 ? sequenceItems.Take(3) : sequenceItems).ToList().SelectMany(item =>
                new List<ICommand>()
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
                });
        }



        private ICommand FadeInItem(string componentId, int duration, int? delay = null)
        {
            return new AnimateItem()
            {
                componentId = componentId,
                easing = "ease-in",
                duration = duration,
                delay = delay ?? 0,
                value = new List<IValue>()
                {
                    new OpacityValue()
                    {
                        from = 0,
                        to = 1
                    }
                }
            };
        }

        private ICommand ScaleFadeInItem(string componentId, int duration, int? delay = null)
        {
            
            var animation = new Parallel()
            {
                commands = new List<ICommand>()
                {
                    new AnimateItem()
                    {
                        componentId = componentId,
                        duration = duration,
                        delay = delay ?? 0,
                        value = new List<IValue>()
                        {
                            new OpacityValue()
                            {
                                from = 0,
                                to = 1
                            }
                        }
                    },
                    new AnimateItem()
                    {
                        componentId = componentId,
                        duration = duration,
                        delay = delay ?? 0,
                        value = new List<IValue>()
                        {
                            new TransitionValue()
                            {
                                from = new List<From>()
                                {
                                    new From()
                                    {
                                        scaleX = 1.2,
                                        scaleY = 1.2
                                    }
                                },
                                to = new List<To>()
                                {
                                    new To()
                                    {
                                        scaleX = 1,
                                        scaleY = 1
                                    }
                                }
                            }
                        }
                    }
                }
            };
           
            return animation;
        }

        public void Dispose()
        {

        }

        public void Run()
        {

        }
    }
}