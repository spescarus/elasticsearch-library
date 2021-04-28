using System;

namespace SP.ElasticSearchLibrary.Search.Exceptions
{
    public sealed class ElasticSearchException : Exception
    {
        public ElasticSearchException(string message)
            : base(message)
        {
        }

        public ElasticSearchException(string    message,
                                      Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
