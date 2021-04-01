using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Extensions;
using AlexaController.Alexa.RequestModel;

namespace AlexaController.EmbyApl.AplResourceManagement
{
    public static class SettingsManager
    {
        public static ISettings RenderSettings(Context context)
        {
            var settings = new Settings();
            if (context.Extensions.Available.ContainsKey("alexaext:smartmotion:10"))
            {
                settings.SmartMotion = new SmartMotion()
                {
                    deviceStateName = "MyDeviceState",
                    wakeWordResponse = WakeWordResponse.followOnWakeWord
                };
            }

            if (context.Extensions.Available.ContainsKey("alexaext:entitysensing:10"))
            {
                settings.EntitySensing = new EntitySensing()
                {
                    primaryUserName = "MyPrimaryUser",
                    entitySensingStateName = "MyEntitySensingState"
                };
            }

            return settings;
        }
    }
}
