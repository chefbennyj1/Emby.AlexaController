using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Alexa.Presentation.DataSources.Transformers;
using AlexaController.EmbyAplDataSource.DataSourceProperties;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Alexa.Presentation.DataSources
{
    public class DataSourceBuilder
    {
        private List<ITransformer> Transformers { get; }
        private IProperties Properties { get; set; }

        public DataSourceBuilder()
        {
            Transformers = new List<ITransformer>();
        }

        public void AddRange(List<ITransformer> transformers)
        {
            Transformers.AddRange(transformers);
        }

        public void Add(ITransformer transformer)
        {
            Transformers.Add(transformer);
        }

        public void Add<T>(Properties<T> properties) where T : class
        {
            Properties = properties;
        }

        public async Task<Dictionary<string, IDataSource>> CreateDataSource(string name)
        {
            return await Task.FromResult(new Dictionary<string, IDataSource>()
            {
                {
                    name,
                    new DataSource()
                    {
                        properties = Properties,
                        transformers = Transformers
                    }
                }
            });
        }
    }
}
