using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.Directives;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Alexa.SpeechSynthesis;
using Document = AlexaController.Alexa.Presentation.APLA.Document;

namespace AlexaController.PresentationManagers
{

    public class AplaRenderDocumentDirectiveManager : Ssml
    {
        public static AplaRenderDocumentDirectiveManager Instance { get; private set; }
        
        public AplaRenderDocumentDirectiveManager()
        {
            Instance = this;
        }

        public async Task<IDirective> GetAudioDirectiveAsync(IDataSource dataSource)
        {
            return await Task.FromResult(new AplaRenderDocumentDirective()
            {
                token    = "AplSpeech",
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
                                new Audio()  { source = "${payload.templateData.properties.audioUrl}"}
                            },
                           
                        }
                        
                    }
                },
                datasources = new Dictionary<string, IDataSource>() { { "templateData", dataSource } }
            });
        }
    }
}
