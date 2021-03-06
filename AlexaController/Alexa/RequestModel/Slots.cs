﻿namespace AlexaController.Alexa.RequestModel
{
    public class Slots
    {
        //Captures words that are anaphors representing an item, such as "this", "that", and "it"
        public slotData Anaphor { get; set; }
        public slotData Episode { get; set; }
        //Captures words that indicate the user is expecting a visual response, such as "show" or "display"
        public slotData VisualModeTrigger { get; set; }
        public slotData Series { get; set; }
        public slotData Movie { get; set; }
        public slotData Device { get; set; }
        public slotData Room { get; set; }
        public slotData ActorName { get; set; }
        public slotData Genre { get; set; }
        public slotData MediaType { get; set; }
        //Converts words that indicate durations ("five minutes") into a numeric duration ("PT5M").
        public slotData Duration { get; set; }
        public slotData SeasonNumber { get; set; }
        public slotData EpisodeNumber { get; set; }
        public slotData MovieCollection { get; set; }
        public slotData ComicBook { get; set; }
        public slotData @object { get; set; }
        public slotData MovieAlternatives { get; set; }
        // ReSharper disable once InconsistentNaming
        public slotData TVAlternatives { get; set; }
        public slotData MediaTypeMovie { get; set; }
        public slotData MediaTypeSeries { get; set; }
    }
}
