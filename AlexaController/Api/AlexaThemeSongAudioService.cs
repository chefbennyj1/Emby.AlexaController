using System;
using System.Collections.Generic;
using System.Text;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Services;

namespace AlexaController.Api
{
    [Route("/AlexaThemeSongAudio", "Get", Summary = "Formatted Media Theme Audio for Alexa")]
    public class AlexaThemeSongAudio : IReturn<object>
    {
        [ApiMember(Name = "Id", Description = "Guid", IsRequired = true, DataType = "Guid", ParameterType = "query", Verb = "GET")]
        public Guid Id { get; set; }
    }

    public class AlexaThemeSongAudioService : IService
    {
        private IFfmpegManager ffmpegManager { get; set; }
        public AlexaThemeSongAudioService(IFfmpegManager ffmpeg)
        {
            ffmpegManager = ffmpeg;
        }
    }
}
