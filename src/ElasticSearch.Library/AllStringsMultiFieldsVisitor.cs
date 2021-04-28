using System.Reflection;
using Nest;

namespace SP.ElasticSearchLibrary
{
    public class AllStringsMultiFieldsVisitor : NoopPropertyVisitor
    {
        /// <exception cref="T:System.Reflection.AmbiguousMatchException">More than one of the requested attributes was found.</exception>
        /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded.</exception>
        public override void Visit(ITextProperty                      type,
                                   PropertyInfo                       propertyInfo,
                                   ElasticsearchPropertyAttributeBase attribute)
        {
            // if a custom attribute has been applied, let it take precedence
            if (propertyInfo.GetCustomAttribute<ElasticsearchPropertyAttributeBase>() == null &&
                propertyInfo.PropertyType                                             == typeof(string))
            {
                type.Fields = new Properties
                {
                    {
                        "keyword", new KeywordProperty
                        {
                            Normalizer = "case_insensitive",
                        }
                    },
                    {
                        "ngram", new TextProperty
                        {
                            Analyzer       = "edge_ngram_analyzer",
                            SearchAnalyzer = "edge_ngram_analyzer"
                        }

                    }
                };
                type.Analyzer       = "standard";
                type.SearchAnalyzer = "standard";
            }

            base.Visit(type, propertyInfo, attribute);
        }
    }
}
