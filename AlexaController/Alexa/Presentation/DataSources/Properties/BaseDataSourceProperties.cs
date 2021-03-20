using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.DataSources.Properties
{
    public abstract class BaseDataSourceProperties<T> : IProperties where T : class
    {
        public T item { get; set; }
        public List<T> items { get; set; }
        public T value { get; set; }

    }
}
