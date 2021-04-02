﻿using AlexaController.Alexa.Presentation.DataSources.Transformers;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.EmbyAplDataSource.DataSourceProperties;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ParameterHidesMember
// ReSharper disable InconsistentNaming

namespace AlexaController.Alexa.Presentation.DataSources
{
    public class Data : IData
    {
        public object type => "object";
        public string objectID                 { get; set; }
        public string description              { get; set; }
        public IProperties properties          { get; set; }
        public List<ITransformer> transformers { get; set; }
       
        public void AddRange(IEnumerable<ITransformer> transformers)
        {
            if (this.transformers is null) this.transformers = new List<ITransformer>();
            this.transformers.AddRange(transformers);
        }
        public void Add(ITransformer transformer)
        {
            if (transformers is null) transformers = new List<ITransformer>();
            transformers.Add(transformer);
        }

        public void Add<T>(Properties<T> properties) where T : class
        {
            this.properties = properties;
        }

        public async Task<IData> Build()
        {
            return await Task.FromResult(new Data()
            {
                properties = properties,
                transformers = transformers
            });
        }
        
    }
}
