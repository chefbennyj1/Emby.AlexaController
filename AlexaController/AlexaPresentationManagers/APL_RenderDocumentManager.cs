using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APLA.AudioFilters;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.Directives;
using AlexaController.Alexa.ResponseModel;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using AlexaController.AlexaPresentationManagers.Imports;
using AlexaController.AlexaPresentationManagers.Layouts;
using AlexaController.AlexaPresentationManagers.Resources;
using AlexaController.AlexaPresentationManagers.VectorGraphics;
using AlexaController.Session;

// ReSharper disable twice InconsistentNaming

/*
 * Echo Display Devices use the **LAN address** for Images when using the skill on the same network as emby server (ex. 192.168.X.XXX:8096)
 * We do not need to serve media requests over https like the documentation implies while using APL 1.1.
 * https://developer.amazon.com/en-US/docs/alexa/alexa-presentation-language/apl-image.html
 */

namespace AlexaController.AlexaPresentationManagers
{
    public class APL_RenderDocumentManager : LayoutFactory
    {
        public static APL_RenderDocumentManager Instance { get; private set; }

        public APL_RenderDocumentManager()
        {
            Instance = this;
        }

        public async Task<IDirective> GetRenderDocumentDirectiveAsync<TProperties>(IDataSource dataSource, IAlexaSession session) where TProperties : class
        {
            var data       = dataSource as DataSource<TProperties>;
            var properties = data?.properties as Properties<TProperties>;

            List<IComponent> layout = null;
            switch (properties?.documentType)
            {
                case RenderDocumentType.GENERIC_VIEW_TEMPLATE:
                    layout = await RenderGenericViewLayout(dataSource);
                    break;
                case RenderDocumentType.MEDIA_ITEM_DETAILS_TEMPLATE:
                    layout = await RenderItemDetailsLayout(dataSource, session);
                    break;
                case RenderDocumentType.MEDIA_ITEM_LIST_SEQUENCE_TEMPLATE:
                    layout = await RenderItemListSequenceLayout(dataSource, session);
                    break;
                case RenderDocumentType.ROOM_SELECTION_TEMPLATE:
                    layout = await RenderRoomSelectionLayout(dataSource, session);
                    break;
                case RenderDocumentType.HELP_TEMPLATE:
                    layout = await RenderHelpViewLayout(dataSource);
                    break;
                default: return null;
            }
            
            return await Task.FromResult(new AplRenderDocumentDirective()
            {
                token = properties.documentType.ToString(),
                document = new Document()
                {
                    theme     = "${payload.templateData.properties.theme}",
                    import    = ImportsFactory.Imports,
                    resources = ResourceFactory.Resources,
                    graphics  = VectorGraphicsFactory.VectorGraphics,
                    commands  = new Dictionary<string, ICommand>()
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

    }
}