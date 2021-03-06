using System.Collections.Generic;

namespace AlexaController.DataSourceProperties
{
    //No constraints on generic property class!
    public class Properties<T> : BaseProperties	 where T :class
    {
        public List<T> similarItems       { get; set; }
        public T item                     { get; set; }
        public List<T> items              { get; set; }
        public T value                    { get; set; }
        public string text                { get; set; }
        public string videoUrl            { get; set; }
        public List<Value> values         { get; set; }
    }
    
    public class Value
    {
        public string value { get; set; }
    }
}
