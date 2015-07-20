using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stubble.Core;
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

        [Fact]
        public void It_Handles_Arrays_Correctly()
        {
            const string json = "{ foo: [ { bar: \"foobar\" } ] }";

            var stubble = new StubbleBuilder()
                                .AddJsonNet()
                                .Build();

            var obj = JsonConvert.DeserializeObject(json);

            var output = stubble.Render("{{#foo}}{{bar}}{{/foo}}", obj);
            Assert.NotNull(output);
            Assert.Equal("foobar", output);
        }

        [Fact]
        public void It_Handles_Nested_Objects()
        {
            const string json = "{ foo: { bar: \"foobar\" } }";

            var stubble = new StubbleBuilder()
                                .AddJsonNet()
                                .Build();

            var obj = JsonConvert.DeserializeObject(json);

            var output = stubble.Render("{{foo.bar}}", obj);
            Assert.NotNull(output);
            Assert.Equal("foobar", output);
        }

        [Theory]
        [InlineData("{ foo: 1 }", (long)1)] //Ints are always longs in Json.Net
        [InlineData("{ foo: \"2\" }", "2")]
        [InlineData("{ foo: 1.01 }", 1.01)]
        [InlineData("{ foo: null }", null)]
        [InlineData("{ foo: true }", true)]
        public void Tokens_Return_Correct_DotNet_Type(string json, object expected)
        {
            var obj = JsonConvert.DeserializeObject(json);

            var value = JsonNet.ValueGetters[typeof(JObject)](obj, "foo");
            Assert.Equal(expected, value);
        }

        [Fact]
        public void It_Handles_DateTimes_Correctly()
        {
            var obj = JsonConvert.DeserializeObject("{ foo: \"2009-02-15T00:00:00Z\" }");

            var value = JsonNet.ValueGetters[typeof(JObject)](obj, "foo");
            Assert.Equal(DateTime.Parse("2009-02-15T00:00:00Z"), value);
        }

        [Fact]
        public void Truthy_Checks_Work_Correctly()
        {
            const string json = "{ showme: false, foo: { bar: \"foobar\" } }";

            var stubble = new StubbleBuilder()
                                .AddJsonNet()
                                .Build();

            var obj = JsonConvert.DeserializeObject(json);

            var output = stubble.Render("{{#showme}}{{foo.bar}}{{/showme}}", obj);
            Assert.NotNull(output);
            Assert.Equal("", output);
        }

        [Fact]
        public void Truthy_Checks_Work_For_Inverted()
        {
            const string json = "{ showme: false, foo: { bar: \"foobar\" } }";

            var stubble = new StubbleBuilder()
                                .AddJsonNet()
                                .Build();

            var obj = JsonConvert.DeserializeObject(json);

            var output = stubble.Render("{{^showme}}{{foo.bar}}{{/showme}}", obj);
            Assert.NotNull(output);
            Assert.Equal("foobar", output);
        }
    }
}
