using System;

namespace SP.ElasticSearchLibrary.Search.Exceptions
{
    public sealed class SearchException : Exception
    {
        public SearchException(string message)
            : base(message)
        {
        }

        public SearchException(string    message,
                               Exception innerException)
            : base(message, innerException)
        {
        }
    }
}