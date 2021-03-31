using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Alexa.Presentation.Sources
{
    public class SourcesBuilder
    {
        private Dictionary<string, IDocument> Sources { get; }

        public SourcesBuilder()
        {
            Sources = new Dictionary<string, IDocument>();
        }

        public void Add(string name, IDocument document)
        {
            Sources.Add(name, document);
        }

        public async Task<Dictionary<string, IDocument>> Create()
        {
            return await Task.FromResult(Sources);
        }
    }
}
