using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using System.Linq;
using IPerson = AlexaController.Alexa.RequestModel.IPerson;

namespace AlexaController.Utils
{
    public class SpeechAuthorization //: IServerEntryPoint
    {
        private IUserManager UserManager { get; }
        public static SpeechAuthorization Instance { get; private set; }

        public SpeechAuthorization(IUserManager userManager)
        {
            UserManager = userManager;
            Instance = this;
        }

        public bool UserPersonalizationProfileExists(IPerson person)
        {
            var config = Plugin.Instance.Configuration;
            return config.UserCorrelations.Exists(u => u.AlexaPersonId == person.personId);
        }

        public User GetRecognizedPersonalizationProfileResult(IPerson person)
        {
            var users = UserManager.Users;
            var defaultUser = users.FirstOrDefault(user => user.Policy.IsAdministrator);
            var config = Plugin.Instance.Configuration;

            if (!config.EnableParentalControlVoiceRecognition) return defaultUser;

            try
            {
                return config.UserCorrelations.Exists(u => u.AlexaPersonId == person.personId)
                    ? UserManager.GetUserById(config.UserCorrelations.FirstOrDefault(u => u.AlexaPersonId == person.personId)?.EmbyUserId)
                    : defaultUser;
            }
            catch
            {
                return defaultUser;
            }

        }
    }
}
