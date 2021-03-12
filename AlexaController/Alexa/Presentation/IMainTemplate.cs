using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation
{
    public interface IMainTemplate 
    {
        List<string> parameters       { get; set; }
        List<IComponent> items        { get; set; }
        IComponent item               { get; set; }
    }
}
