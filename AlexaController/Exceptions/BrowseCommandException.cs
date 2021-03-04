using System;

namespace AlexaController.Exceptions
{
    public class BrowseCommandException : Exception
    {
        public BrowseCommandException()
        {
        }

        public BrowseCommandException(string message) : base(message)
        {
        }

        public BrowseCommandException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
