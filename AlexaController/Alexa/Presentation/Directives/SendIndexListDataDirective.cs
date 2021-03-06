﻿using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.Directives
{
    public class SendIndexListDataDirective : IDirective
    {
        public string type => "Alexa.Presentation.APL.SendIndexListData";
        public string token { get; set; }
        public string correlationToken { get; set; }

        public string listId { get; set; }
        public int listVersion { get; set; }
        public int startIndex { get; set; }

        public string minimumInclusiveIndex { get; set; }

        public string maximumInclusiveIndex { get; set; }
        public List<object> items { get; set; }

        public Dictionary<string, ITemplateData> datasources { get; set; }
    }
}
