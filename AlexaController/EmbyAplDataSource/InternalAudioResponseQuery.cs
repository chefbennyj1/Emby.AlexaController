using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using System;
using System.Collections.Generic;

namespace AlexaController.EmbyAplDataSource
{
    public class InternalAudioResponseQuery
    {
        public SpeechResponseType SpeechResponseType { get; set; }
        public List<BaseItem> items { get; set; }
        public BaseItem item { get; set; }
        public DateTime date { get; set; }
        public IAlexaSession session { get; set; }
        public bool deviceAvailable { get; set; } = true;
    }
}