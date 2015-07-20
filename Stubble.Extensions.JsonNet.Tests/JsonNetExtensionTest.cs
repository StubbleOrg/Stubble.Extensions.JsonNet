using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stubble.Core;
using Stubble.Extensions.JsonNet;
using Xunit;

namespace Stubble.Extensions.JsonNet.Tests
{
    public class JsonNetExtensionTest
    {
        [Fact]
        public void It_Can_Get_Values_From_JTokens()
        {
            const string json = "{ foo: \"bar\" }";

            var stubble = new StubbleBuilder()
                                .AddJsonNet()
                                .Build();

            var obj = JsonConvert.DeserializeObject(json);

            var output = stubble.Render("{{foo}}", obj);
            Assert.Equal("bar", output);
        }

        [Fact]
        public void It_Doesnt_Throw_When_No_Value_Exists()
        {
            const string json = "{ foo: \"bar\" }";

            var stubble = new StubbleBuilder()
                                .AddJsonNet()
                                .Build();

            var obj = JsonConvert.DeserializeObject(json);

            var output = stubble.Render("{{foo2}}", obj);
            Assert.Equal("", output);
        }

        public void Tokens_Return_Correct_Type()
        {
        }
    }
}
