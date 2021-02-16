using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation
{
    public interface IMainTemplate 
    {
        List<string> parameters  { get; set; }
        List<IItem> items        { get; set; }
        IItem item               { get; set; }
    }
}
