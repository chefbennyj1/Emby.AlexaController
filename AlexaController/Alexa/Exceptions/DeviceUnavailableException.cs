using System;

namespace AlexaController.Alexa.Exceptions
{
    public class DeviceUnavailableException : Exception
    {
        public DeviceUnavailableException()
        {
        }

        public DeviceUnavailableException(string message) : base(message)
        {
        }

        public DeviceUnavailableException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
