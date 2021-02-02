using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

    [Route("/MovingFloor", "Get", Summary = "video backdrop")]
    public class MovingFloor : IReturn<object>
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

        public async Task<object> Get(NowShowingImage request) => 
            await Task.Factory.StartNew(() => 
                GetEmbeddedResourceStream("nowShowing.png", "image/png"));

        public async Task<object> Get(EmptyPngImage request) => 
            await Task.Factory.StartNew(() => 
                GetEmbeddedResourceStream("empty.png", "image/png"));

        public async Task<object> Get(FiberOptic request) => 
            await Task.Factory.StartNew(() => 
                GetEmbeddedResourceStream("fiberOptics.mp4", "video/mp4"));

        public async Task<object> Get(Question request) => 
            await Task.Factory.StartNew(() => 
                GetEmbeddedResourceStream("ICON_VERSION6.mp4", "video/mp4"));

        public async Task<object> Get(MovieLibrary request) => 
            await Task.Factory.StartNew(() => 
                GetEmbeddedResourceStream("MoviesLibrary.mp4", "video/mp4"));

        public async Task<object> Get(Particles request) => 
            await Task.Factory.StartNew(() => 
                GetEmbeddedResourceStream("particles.mp4", "video/mp4"));

        public async Task<object> Get(MovingFloor request) => 
            await Task.Factory.StartNew(() => 
                GetEmbeddedResourceStream("MovingFloor.mp4", "video/mp4"));


        private object GetEmbeddedResourceStream(string resourceName, string contentType)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name     = assembly.GetManifestResourceNames().Single(s => s.EndsWith(resourceName));

            return ResultFactory.GetResult(Request, GetType().Assembly.GetManifestResourceStream(name), contentType);
        }
    }
}

