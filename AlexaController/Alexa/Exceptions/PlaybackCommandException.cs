using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Exceptions
{
    public class PlaybackCommandException : Exception
    {
        public PlaybackCommandException()
        {
        }

        public PlaybackCommandException(string message) : base(message)
        {
        }

        public PlaybackCommandException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
