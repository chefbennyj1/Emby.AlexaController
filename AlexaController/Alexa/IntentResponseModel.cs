using System;
using System.Linq;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable once UnusedMemberInSuper.Global
// ReSharper disable once TooManyArguments
// ReSharper disable once ComplexConditionExpression

namespace AlexaController.Alexa
{
    public abstract class IntentResponseModel
    {
        public abstract string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, 
            ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager);

        //protected readonly Func<Intent, AlexaSession, string> GetRoom = (intent, session) 
        //    => (intent.slots.Room.value ?? session.room) ?? string.Empty;

        //protected Room GetRoomTest(Intent intent, AlexaSession session)
        //{
        //    if (intent.slots.Room.value is null) return session.room2 ?? Room.Empty;
        //    var room = ValidateRoomConfiguration(intent.slots.Room.value, Plugin.Instance.Configuration) 
        //        ? Plugin.Instance.Configuration.Rooms.FirstOrDefault(r => r.Name == intent.slots.Room.value) 
        //        : Room.Empty;
        //    ServerEntryPoint.Instance.Log.Info("ALEXA WILL RETURN: " + room.Name);
        //    return room;
        //}
            

        //protected readonly Func<string, PluginConfiguration, bool> ValidateRoomConfiguration = (room, config) 
        //    => config.Rooms.Exists(r => string.Equals(StringNormalization.NormalizeText(r.Name), room, 
        //        StringComparison.InvariantCultureIgnoreCase));

        ////protected Room GetRoom(Intent intent, AlexaSession session)
        ////{
        ////    if (intent.slots.Room.value is null)
        ////    {
        ////        if (session.room is null)
        ////        {
        ////            return null;
        ////        }
        ////    }
        ////}
    }
}
