using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Luaon.Json;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTestProject1.Tests
{

    public class JsonLuaWriterTests : TestsBase
    {
        /// <inheritdoc />
        public JsonLuaWriterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void SerializationTest()
        {
            var serializer = new JsonSerializer();
            var obj = new
            {
                IntValue = 123,
                FloatValue = 456.78,
                StringValue = "test",
                Value1 = (object)null,
                Value2 = new
                {
                    Value21 = 12345.67M
                },
                Value3 = new Dictionary<string, int>
                {
                    {"A", 1},
                    {"B", 2},
                    {"C", 3},
                    {"while", 4}
                }
            };
            using (var sw = new StringWriter())
            {
                using (var lw = new JsonLuaWriter(sw) {CloseOutput = false})
                    serializer.Serialize(lw, obj, typeof(object));
                Assert.Equal(
                    "{IntValue=123,FloatValue=456.78,StringValue=\"test\",Value1=nil,Value2={Value21=12345.67},Value3={A=1,B=2,C=3,[\"while\"]=4}}",
                    sw.ToString());
            }

            using (var sw = new StringWriter())
            {
                using (var lw = new JsonLuaWriter(sw) {Formatting = Formatting.Indented, CloseOutput = false})
                    serializer.Serialize(lw, obj, typeof(object));
                Assert.Equal(@"{
  IntValue = 123,
  FloatValue = 456.78,
  StringValue = ""test"",
  Value1 = nil,
  Value2 = {
    Value21 = 12345.67
  },
  Value3 = {
    A = 1,
    B = 2,
    C = 3,
    [""while""] = 4
  }
}".Replace("\r\n", sw.NewLine), sw.ToString());
            }
        }

    }
}
