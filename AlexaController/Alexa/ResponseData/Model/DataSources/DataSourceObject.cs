﻿using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.ResponseData.Model.DataSources
{
    public class DataSourceObject : IDataSource
    {
        public object type => "object";
        public Properties properties { get; set; }
        public string objectID { get; set; }
        public string description { get; set; }
        //TODO: Transformers
    }

    public class Properties
    {
        public Item item { get; set; }
        public List<Item> items { get; set; }
    }

    public class Item
    {
        public string source { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public string index { get; set; }
        public string premiereDate { get; set; }
    }
}