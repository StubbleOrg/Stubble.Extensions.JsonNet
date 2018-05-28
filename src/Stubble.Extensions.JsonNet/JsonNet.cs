using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Stubble.Core.Settings;

namespace Stubble.Extensions.JsonNet
{
    public static class JsonNet
    {
        public static RendererSettingsBuilder AddJsonNet(this RendererSettingsBuilder builder)
        {
            foreach(var getter in ValueGetters)
            {
                builder.AddValueGetter(getter.Key, getter.Value);
            }

            return builder;
        }

        internal static readonly Dictionary<Type, RendererSettingsDefaults.ValueGetterDelegate> ValueGetters = new Dictionary<Type, RendererSettingsDefaults.ValueGetterDelegate>
        {
            {
                typeof (JObject), (value, key, ignoreCase) =>
                {
                    var token = (JObject)value;
                    var comparison =
                        ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    var childToken = token.GetValue(key, comparison);

                    if (childToken == null) return null;

                    switch (childToken.Type)
                    {
                        case JTokenType.Array:
                        case JTokenType.Object:
                        case JTokenType.Property:
                            return childToken;
                    }

                    var jValue = childToken as JValue;

                    return jValue?.Value;
                }
            },
        };
    }
}
