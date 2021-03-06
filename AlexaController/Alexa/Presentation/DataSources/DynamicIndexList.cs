﻿using System.Collections.Generic;
using AlexaController.DataSourceProperties;

namespace AlexaController.Alexa.Presentation.DataSources
{
    public class DynamicIndexList : IDataSource
    {
        public object type => nameof(DynamicIndexList);
        public string objectID                 { get; set; }
        public string description              { get; set; }
        public string listId                   { get; set; }
        public int startIndex                  { get; set; }
        public int minimumInclusiveIndex       { get; set; }
        public int maximumExclusiveIndex       { get; set; }
        public IProperties properties          { get; set; }
        public List<ITransformer> transformers { get; set; }
        public List<object> items              { get; set; }
    }

    public class DynamicIndexListProperties : IProperties
    {
        public RenderDocumentType documentType { get; set; }
    }
}