﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Silence
    {
        public object type => nameof(Silence);
        public int duration { get; set; }
    }
}