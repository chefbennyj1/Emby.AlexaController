using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    public class UserEventReadOverview : IUserEventResponse
    {
        public UserEventReadOverview(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }
        public IAlexaRequest AlexaRequest { get; }
        public async Task<string> Response()
        {
            var request  = AlexaRequest.request;
            var source   = request.source;
            var session  = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var baseItem = ServerQuery.Instance.GetItemById(source.id);
            

            var aplaDataSource = await AplaDataSourceManager.Instance.ReadItemOverview(baseItem);
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                directives = new List<IDirective>()
                {
                    await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                },
                //outputSpeech = new OutputSpeech()
                //{
                //    phrase = baseItem.Overview,
                //},
                SpeakUserName = false,
                shouldEndSession = null

            }, session); 
        
        }
    }
}
