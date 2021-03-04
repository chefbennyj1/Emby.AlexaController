using System;
using System.Collections.Generic;
using System.Text;
using AlexaController.Alexa.Presentation.DataSources;

namespace AlexaController.DataSourceProperties
{
    public class BaseProperties : IProperties
    {
        public RenderDocumentType documentType { get; set; }
    }
}
