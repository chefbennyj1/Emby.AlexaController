using System;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;

// ReSharper disable once TooManyDependencies

namespace AlexaController.Api
{
    [Route("/FlashBriefNewMovies", "GET", Summary = "Alexa Flash Briefing Endpoint for new movies")]
    public class FlashBriefingRequest : IReturn<string>
    {
        public string uid            { get; set; }
        public DateTime updateDate   { get; set; }
        public string titleText      { get; set; }
        public string mainText       { get; set; }
        public string redirectionUrl { get; set; }

        public string data { get; set; }
    }
    
    public class AlexaFlashBriefingService : IService
    {
        private IJsonSerializer JsonSerializer { get; }
        
        public AlexaFlashBriefingService(IJsonSerializer json)
        {
            JsonSerializer = json;
        }
        public string Get(FlashBriefingRequest request)
        {
            var d = new FlashBriefingRequest()
            {
                mainText       = "This is a test",
                redirectionUrl = "https://unityhome.online",
                titleText      = "This is a test",
                uid            = Guid.NewGuid().ToString(),
                updateDate     = DateTime.Now
            };

            return JsonSerializer.SerializeToString(d);
        }

    }
}
