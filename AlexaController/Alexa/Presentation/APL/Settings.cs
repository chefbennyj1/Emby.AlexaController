using AlexaController.Alexa.Presentation.APL.Extensions;

namespace AlexaController.Alexa.Presentation.APL
{
    public class Settings : ISettings
    {
        public int idleTimeout { get; set; }
        public SmartMotion SmartMotion { get; set; }
        public EntitySensing EntitySensing { get; set; }
    }
}