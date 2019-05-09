using System;
using System.IO;
using Luaon;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTestProject1.Tests
{
    public class LuaTableTextWriterTest : TestsBase
    {

        public LuaTableTextWriterTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void WriteLiteralTest()
        {
            using (var sw = new StringWriter())
            using (var tw = new LuaTableTextWriter(sw))
            {
                tw.WriteStartTable();
                tw.WriteLiteral('@');
                tw.WriteLiteral("\r\n\t\f\b?{\\r\\n\"\'");
                tw.WriteLiteral(true);
                tw.WriteLiteral(10);
                tw.WriteLiteral(10.99);
                tw.WriteLiteral(0.99);
                tw.WriteLiteral(0.000000000000000001d);
                tw.WriteLiteral(0.000000000000000001m);
                tw.WriteLiteral((string)null);
                tw.WriteLiteral((object)null);
                tw.WriteLiteral("This is a string.");
                tw.WriteNil();
                tw.WriteEndTable();
                tw.Flush();
                Assert.Equal("{64,\"\\r\\n\\t\\f\\b?{\\r\\n\\\"\'\",true,10,10.99,0.99,1E-18,0.000000000000000001,\"\",nil,\"This is a string.\",nil}",
                    sw.ToString());
            }
        }

        [Fact]
        public void WriteKeyedTest()
        {
            using (var sw = new StringWriter())
            using (var tw = new LuaTableTextWriter(sw))
            {
                tw.WriteStartTable();
                tw.WriteKey("Test");
                tw.WriteLiteral("value");
                tw.WriteKey(123);
                tw.WriteLiteral(456);
                tw.WriteKey(TimeSpan.FromHours(3.2));
                tw.WriteNil();
                tw.WriteKey("while");
                tw.WriteNil();
                tw.WriteKey("function");
                tw.WriteNil();

                // Though it's meaningless…
                tw.WriteStartKey();
                tw.WriteStartTable();
                tw.WriteLiteral(1);
                tw.WriteLiteral(2);
                tw.WriteEndTable();
                tw.WriteEndKey();
                tw.WriteLiteral(123);

                tw.WriteEndTable();
                tw.Flush();
                Assert.Equal("{Test=\"value\",[123]=456,[\"03:12:00\"]=nil,[\"while\"]=nil,[\"function\"]=nil,[{1,2}]=123}", sw.ToString());
            }
        }

        [Fact]
        public void WriteNestedTableTest()
        {
            using (var sw = new StringWriter())
            using (var tw = new LuaTableTextWriter(sw))
            {
                tw.WriteStartTable();

                tw.WriteStartTable();
                tw.WriteLiteral(1);
                tw.WriteLiteral(2);
                tw.WriteLiteral(3);
                tw.WriteEndTable();

                tw.WriteLiteral(4);
                tw.WriteKey("Named5");
                tw.WriteLiteral(5);

                tw.WriteStartTable();
                tw.WriteLiteral(6);
                tw.WriteKey("Named7");
                tw.WriteLiteral(7);
                tw.WriteEndTable();

                tw.WriteEndTable();
                tw.Flush();
                Assert.Equal("{{1,2,3},4,Named5=5{6,Named7=7}}", sw.ToString());
            }
        }

    }
}
