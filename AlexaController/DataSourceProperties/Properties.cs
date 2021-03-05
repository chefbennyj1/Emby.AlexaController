using System.Collections.Generic;

namespace AlexaController.DataSourceProperties
{
    //No constraints on generic property class!
    public class Properties<T> : BaseProperties where T :class
    {
        public List<T> similarItems       { get; set; }
        public T item                     { get; set; }
        public List<T> items              { get; set; }
        public T value                    { get; set; }
        public List<Value<string>> values      { get; set; }
    }
    
    public class Value<T>
    {
        public T value { get; set; }
    }
}
