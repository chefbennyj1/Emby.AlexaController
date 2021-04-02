using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Alexa.Presentation.Sources
{
    public class Sources
    {
        private Dictionary<string, IDocument> sources { get; }

        public Sources()
        {
            sources = new Dictionary<string, IDocument>();
        }

        public void Add(string name, IDocument document)
        {
            sources.Add(name, document);
        }

        public async Task<Dictionary<string, IDocument>> Build()
        {
            return await Task.FromResult(sources);
        }
    }
}
