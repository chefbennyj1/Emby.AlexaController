using System;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments
// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    [Intent]
    public class YesIntent : IntentResponseModel
    {
        public override string Response(AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient,
            ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            throw new NotImplementedException();
        }
    }
}
