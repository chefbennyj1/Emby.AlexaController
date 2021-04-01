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
using AlexaController.Alexa.Presentation.Sources;
using AlexaController.Alexa.ResponseModel;
using AlexaController.EmbyApl.AplResourceManagement;
using AlexaController.EmbyAplDataSource.DataSourceProperties;
using AlexaController.Session;

/*
 * Echo Display Devices use the **LAN address** for Images when using the skill on the same network as emby service (ex. 192.168.X.XXX:8096)
 * We do not need to serve media requests over https like the documentation implies while using APL 1.1.
 * https://developer.amazon.com/en-US/docs/alexa/alexa-presentation-language/apl-image.html
 */

namespace AlexaController.EmbyApl
{
    public class RenderDocumentDirectiveManager
    {
        public static RenderDocumentDirectiveManager Instance { get; private set; }

        public RenderDocumentDirectiveManager()
        {
            Instance = this;
        }

        public async Task<IDirective> RenderVisualDocumentDirectiveAsync<T>(Properties<T> properties, IAlexaSession session) where T : class
        {
            var dataSourceBuilder = new DataSourceBuilder();
            dataSourceBuilder.Add(properties);
            
            var sources = new SourcesBuilder();
            sources.Add("buttonPressEffectApla", new Alexa.Presentation.APLA.Document()
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
            });

            switch (properties?.documentType)
            {
                case RenderDocumentType.MEDIA_ITEM_DETAILS_TEMPLATE:
                    dataSourceBuilder.Add(new TextToSpeechTransformer()
                    {
                        inputPath = "item.overview",
                        outputName = "readOverview"
                    });
                    break;
                case RenderDocumentType.MEDIA_ITEM_LIST_SEQUENCE_TEMPLATE:
                    dataSourceBuilder.Add(new AplaSpeechTransformer()
                    {
                        template = "buttonPressEffectApla",
                        outputName = "buttonPressEffect"
                    });
                    break;
                case RenderDocumentType.HELP_TEMPLATE:
                    dataSourceBuilder.Add(new TextToSpeechTransformer()
                    {
                        inputPath = "values[*].value",
                        outputName = "helpPhrase"
                    });
                    break;
                case RenderDocumentType.GENERIC_VIEW_TEMPLATE:
                    break;
                case RenderDocumentType.ROOM_SELECTION_TEMPLATE:
                    break;
                default: return null;
            }

            return await Task.FromResult(new AplRenderDocumentDirective()
            {
                token = properties.documentType.ToString(),
                // ReSharper disable once RedundantNameQualifier
                document = new Alexa.Presentation.APL.Document()
                {
                    theme      = "${payload.templateData.properties.theme}",
                    extensions = ExtensionsManager.RenderExtensionsList(session.context),
                    settings   = SettingsManager.RenderSettings(session.context),
                    import     = ImportsManager.RenderImportsList,
                    resources  = ResourcesManager.RenderResourcesList,
                    graphics   = VectorGraphicsManager.RenderVectorGraphicsDictionary,
                    commands   = new Dictionary<string, ICommand>()
                    {
                        { nameof(Animations.ScaleInOutOnPress), await Animations.ScaleInOutOnPress() },
                        { nameof(Animations.FadeIn), await Animations.FadeIn() }
                    },
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        items = await Layouts.RenderLayoutComponents(properties, session)
                    }
                },
                datasources = await dataSourceBuilder.CreateDataSource("templateData"),
                sources = await sources.Create()
            });
        }

        public async Task<IDirective> RenderAudioDocumentDirectiveAsync(Properties<string> properties)
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
                datasources = await dataSource.CreateDataSource("templateData")
            });
        }
    }
}