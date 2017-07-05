using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Stubble.Core.Interfaces;
using Stubble.Core.Settings;

namespace Stubble.Extensions.JsonNet
{
    public static class JsonNet
    {
        public static IRendererSettingsBuilder<IStubbleBuilder<T>> AddJsonNet<T>(this IRendererSettingsBuilder<IStubbleBuilder<T>> builder)
        {
            foreach(var getter in ValueGetters)
            {
                builder.AddValueGetter(getter.Key, getter.Value);
            }

            return builder;
        }

        public static IStubbleBuilder<T> AddJsonNet<T>(this IStubbleBuilder<T> builder)
        {
            var settingsBuilder = builder as IRendererSettingsBuilder<IStubbleBuilder<T>>;
            settingsBuilder.AddJsonNet();
            return builder;
        }

        internal static readonly Dictionary<Type, Func<object, string, object>> ValueGetters = new Dictionary<Type, Func<object, string, object>>
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
    }
}
