using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Stubble.Core.Interfaces;

namespace Stubble.Extensions.JsonNet
{
    public static class JsonNet
    {
        internal static readonly IDictionary<Type, Func<object, string, object>> ValueGetters = new Dictionary<Type, Func<object, string, object>>
        {
            {
                typeof (JObject), (value, key) =>
                {
                    var token = (JObject)value;
                    var childToken = token[key];

                    if (childToken == null) return null;

                    switch (childToken.Type)
                    {
                        case JTokenType.Array:
                        case JTokenType.Object:
                        case JTokenType.Property:
                            return childToken;
                    }

                    var jValue = childToken as JValue;

                    return jValue != null ? jValue.Value : null;
                }
            },
        };

        public static IStubbleBuilder AddJsonNet(this IStubbleBuilder builder)
        {
            foreach (var getter in ValueGetters)
            {
                builder.AddValueGetter(getter);
            }

            return builder;
        }
    }
}
