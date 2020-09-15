using System.Linq;
using System.Reflection;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;

// ReSharper disable TooManyChainedReferences
// ReSharper disable MethodNameNotMeaningful

namespace AlexaController.Api
{
    [Route("/NowShowingPng", "Get", Summary = "Empty png resource for video backdrops")]
    public class NowShowingImage : IReturn<object>
    {
        public object img { get; set; }
    }

    [Route("/EmptyPng", "Get", Summary = "Empty png resource for video backdrops")]
    public class EmptyPngImage : IReturn<object>
    {
        public object img { get; set; }
    }

    [Route("/FiberOptics", "Get", Summary = "video backdrop")]
    public class FiberOptic : IReturn<object>
    {
        public object mp4 { get; set; }
    }

    [Route("/Question", "Get", Summary = "video backdrop")]
    public class Question : IReturn<object>
    {
        public object mp4 { get; set; }
    }

    [Route("/MoviesLibrary", "Get", Summary = "video backdrop")]
    public class MovieLibrary : IReturn<object>
    {
        public object mp4 { get; set; }
    }

    [Route("/Particles", "Get", Summary = "video backdrop")]
    public class Particles : IReturn<object>
    {
        public object mp4 { get; set; }
    }
    public class AlexaBackgroundImageService : IService, IHasResultFactory
    {
        public IRequest Request                 { get; set; }
        public IHttpResultFactory ResultFactory { get; set; }

        
        public AlexaBackgroundImageService(IHttpResultFactory resultFactory)
        {
            ResultFactory = resultFactory;
        }

        public object Get(NowShowingImage request) => GetEmbeddedResourceStream("nowShowing.png", "image/png");

        public object Get(EmptyPngImage request) => GetEmbeddedResourceStream("empty.png", "image/png");
        
        public object Get(FiberOptic request) => GetEmbeddedResourceStream("fiberOptics.mp4", "video/mp4");
        
        public object Get(Question request) => GetEmbeddedResourceStream("ICON_VERSION6.mp4", "video/mp4");
        
        public object Get(MovieLibrary request) => GetEmbeddedResourceStream("MoviesLibrary.mp4", "video/mp4");
        
        public object Get(Particles request) => GetEmbeddedResourceStream("particles.mp4", "video/mp4");
        
        
        private object GetEmbeddedResourceStream(string resourceName, string contentType)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name     = assembly.GetManifestResourceNames().Single(s => s.EndsWith(resourceName));

            return ResultFactory.GetResult(Request, GetType().Assembly.GetManifestResourceStream(name), contentType);
        }
    }
}

