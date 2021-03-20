using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APLA.AudioFilters;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Transformers;
using AlexaController.Alexa.Presentation.Directives;
using AlexaController.Alexa.ResponseModel;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using AlexaController.Session;

/*
 * Echo Display Devices use the **LAN address** for Images when using the skill on the same network as emby service (ex. 192.168.X.XXX:8096)
 * We do not need to serve media requests over https like the documentation implies while using APL 1.1.
 * https://developer.amazon.com/en-US/docs/alexa/alexa-presentation-language/apl-image.html
 */

namespace AlexaController.AlexaPresentationManagers
{
    public class RenderDocumentDirectiveFactory 
    {
        public static RenderDocumentDirectiveFactory Instance { get; private set; }

        public RenderDocumentDirectiveFactory()
        {
            Instance = this;
        }

        public async Task<IDirective> GetRenderDocumentDirectiveAsync<T>(Properties<T> properties, IAlexaSession session) where T : class
        {
            List<IComponent> layout;

            var dataSource = new DataSourceBuilder();
            dataSource.Add(properties);

            switch (properties?.documentType)
            {
                case RenderDocumentType.GENERIC_VIEW_TEMPLATE:
                    layout = await Layouts.RenderGenericViewLayout();
                    break;
                case RenderDocumentType.MEDIA_ITEM_DETAILS_TEMPLATE:
                    layout = await Layouts.RenderItemDetailsLayout(properties as Properties<MediaItem>, session);
                    dataSource.Add(new TextToSpeechTransformer()
                    {
                        inputPath  = "item.overview",
                        outputName = "readOverview"
                    });
                    break;
                case RenderDocumentType.MEDIA_ITEM_LIST_SEQUENCE_TEMPLATE:
                    layout = await Layouts.RenderItemListSequenceLayout(properties as Properties<MediaItem>, session);
                    dataSource.Add(new AplaSpeechTransformer()
                    {
                        template   = "buttonPressEffectApla",
                        outputName = "buttonPressEffect"
                    });
                    break;
                case RenderDocumentType.ROOM_SELECTION_TEMPLATE:
                    layout = await Layouts.RenderRoomSelectionLayout(session);
                    break;
                case RenderDocumentType.HELP_TEMPLATE:
                    layout = await Layouts.RenderHelpViewLayout();
                    dataSource.Add(new TextToSpeechTransformer()
                    {
                        inputPath  = "values[*].value",
                        outputName = "helpPhrase"
                    });
                    break;
                default: return null;
            }

            
            return await Task.FromResult(new AplRenderDocumentDirective()
            {
                token = properties.documentType.ToString(),
                // ReSharper disable once RedundantNameQualifier
                document = new Alexa.Presentation.APL.Document()
                {
                    theme     = "${payload.templateData.properties.theme}",
                    import    = Imports.GetImports,
                    resources = Resources.GetResources,
                    graphics  = VectorGraphics.GetVectorGraphics,
                    commands  = new Dictionary<string, ICommand>()
                    {
                        { nameof(Animations.ScaleInOutOnPress), await Animations.ScaleInOutOnPress() },
                        { nameof(Animations.FadeIn), await Animations.FadeIn() }
                    },
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = layout
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { { "templateData", await dataSource.Create() } },
                sources     = new Dictionary<string, IDocument>()
                {
                    { "buttonPressEffectApla", new Alexa.Presentation.APLA.Document()
                        {
                            mainTemplate = new MainTemplate()
                            {
                                parameters = new List<string>() { "payload" },
                                item = new Audio()
                                {
                                    source = "soundbank://soundlibrary/camera/camera_15",
                                    filter = new List<IFilter>() { new Volume() { amount = 0.2 } }
                                }
                            }
                        }
                    }
                }
            });
        }

        public async Task<IDirective> GetAudioDirectiveAsync(Properties<string> properties)
        {
            var dataSource = new DataSourceBuilder();
            dataSource.Add(properties);

            return await Task.FromResult(new AplaRenderDocumentDirective()
            {
                token = "AplAudioSpeech",
                document = new Alexa.Presentation.APLA.Document()
                {
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        item = new Mixer()
                        {
                            items = new List<AudioBaseComponent>()
                            {
                                new Speech() { content = "<speak>${payload.templateData.properties.value}</speak>" },
                                new Audio() { source = "${payload.templateData.properties.audioUrl}" }
                            }
                        }
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { { "templateData", await dataSource.Create() } }
            });
        }
    }
}