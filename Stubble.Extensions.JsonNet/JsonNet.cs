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
        public static IStubbleBuilder AddJsonNet(this IStubbleBuilder builder)
        {
            return builder
                .AddValueGetter(typeof (JObject), (value, key) =>
                {
                    var token = (JObject)value;
                    var jValue = (JValue)token[key];

                    return jValue != null ? jValue.Value : null;
                });
        }
    }
}
