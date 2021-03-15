using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.Directives;
using AlexaController.Alexa.ResponseModel;
using Document = AlexaController.Alexa.Presentation.APLA.Document;

namespace AlexaController.AlexaPresentationManagers
{
    public class APLA_RenderDocumentManager 
    {
        public static APLA_RenderDocumentManager Instance { get; private set; }

        public APLA_RenderDocumentManager()
        {
            Instance = this;
        }

        public async Task<IDirective> GetAudioDirectiveAsync(IDataSource dataSource)
        {
            return await Task.FromResult(new AplaRenderDocumentDirective()
            {
                token = "AplAudioSpeech",
                document = new Document()
                {
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        item = new Mixer()
                        {
                            items = new List<AudioBaseComponent>()
                            {
                                new Speech() { content = "<speak>${payload.templateData.properties.value}</speak>" },
                                new Audio()  { source  = "${payload.templateData.properties.audioUrl}"}
                            }
                        }
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { { "templateData", dataSource } }
            });
        }
    }
}
