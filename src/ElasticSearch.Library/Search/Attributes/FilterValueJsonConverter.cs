using System;
using Newtonsoft.Json.Linq;
using SP.ElasticSearchLibrary.Search.Args.Enums;
using SP.ElasticSearchLibrary.Search.Args.FilterValues;

namespace SP.ElasticSearchLibrary.Search.Attributes
{
    public class FilterValueJsonConverter : JsonCreationConverter<FilterValue>
    {
        protected override FilterValue Create(Type    objectType,
                                              JObject jObject)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));

            FileterValueType? type = null;


            if (jObject.ContainsKey("type"))
            {
                type = Enum.Parse<FileterValueType>(jObject["type"]
                                                       .ToString(), true);
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return type switch
            {
                FileterValueType.Numeric => new NumericFilterValue(),
                FileterValueType.Date    => new DateFilterValue(),
                FileterValueType.Text    => new TextFilterValue(),
                FileterValueType.Guid    => new GuidFilterValue(),
                FileterValueType.Boolean => new BooleanFilterValue(),
                _                        => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
