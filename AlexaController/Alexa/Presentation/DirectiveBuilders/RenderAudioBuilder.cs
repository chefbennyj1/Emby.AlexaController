using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Session;
using Document = AlexaController.Alexa.Presentation.APLA.Document;

namespace AlexaController.Alexa.Presentation.DirectiveBuilders
{
    public class RenderAudioBuilder
    {
        public static RenderAudioBuilder Instance { get; set; }
        
        public RenderAudioBuilder()
        {
            Instance = this;
        }

        public async Task<IDirective> GetAudioDirectiveAsync(RenderAudioTemplate template, IAlexaSession session)
        {
            return null;
        }

        public async Task<IDirective> GetApologeticSpeech()
        {
            return await Task.FromResult(new Directive()
            {
                type = "Alexa.Presentation.APLA.RenderDocument",
                token = "ApologeticSpeech",
                document = new Document()
                {
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>()
                        {
                            "payload"
                        },
                        item = new Selector()
                        {
                            items = new List<AudioItem>()
                            {
                                new Speech()
                                {
                                    content = "<speak>Hello, this is a test</speak>"
                                }
                            }
                        }
                        
                    }
                }
            });
        }

    }
}
