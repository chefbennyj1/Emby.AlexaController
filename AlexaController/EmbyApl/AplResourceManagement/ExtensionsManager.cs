using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Extensions;
using AlexaController.Alexa.RequestModel;
using System.Collections.Generic;

namespace AlexaController.EmbyApl.AplResourceManagement
{
    public static class ExtensionsManager
    {
        public static List<IExtension> RenderExtensionsList(Context context)
        {
            var extensions = new List<IExtension>();
            if (context.Extensions.Available.ContainsKey("alexaext:smartmotion:10"))
            {
                extensions.Add(new Extension()
                {
                    name = nameof(SmartMotion),
                    uri = "alexaext:smartmotion:10"
                });
            }

            if (context.Extensions.Available.ContainsKey("alexaext:entitysensing:10"))
            {
                extensions.Add(new Extension()
                {
                    name = nameof(EntitySensing),
                    uri = "alexaext:entitysensing:10"
                });
            }

            return extensions;
        }
    }
}
