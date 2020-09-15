using System.IO;
using System.Linq;
using System.Reflection;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;

// ReSharper disable TooManyChainedReferences
// ReSharper disable MethodNameNotMeaningful
// ReSharper disable InconsistentNaming
// ReSharper disable once CollectionNeverUpdated.Global

namespace AlexaController.Api
{
    [Route("/InteractionModel", "GET", Summary = "Alexa Interaction Model")]
    public class AlexaInteractionModel : IReturn<string>
    {
        public string InteractionModel { get; set; }
    }
    public class AlexaInteractionModelService : IService
    {
        private IJsonSerializer JsonSerializer { get; }

        public AlexaInteractionModelService(IJsonSerializer json)
        {
            JsonSerializer = json;
        }

        public string Get(AlexaInteractionModel request)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith("InteractionModel.json"));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
                if (!(stream is null))
                    using (var reader = new StreamReader(stream))
                    {
                        return JsonSerializer.SerializeToString(new AlexaInteractionModel()
                            { InteractionModel = reader.ReadToEnd() });
                    }
            return string.Empty;
        }
    }
}
