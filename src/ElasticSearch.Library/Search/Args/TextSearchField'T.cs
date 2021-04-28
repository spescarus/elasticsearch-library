using System;
using System.Linq.Expressions;
using Nest;

namespace SP.ElasticSearchLibrary.Search.Args
{
    public sealed class TextSearchField<T> where T : class
    {
        public Expression<Func<T, dynamic>> Path { get; set; }

        public Expression<Func<T, Field>> Field { get; set; }

        public bool Strict { get; set; }
    }
}
