using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Luaon;
using Xunit;

namespace XUnitTestProject1.Tests
{
    public class LuaTableTextReaderTests
    {

        private void AssertNextToken(LuaTableTextReader reader, LuaTableReaderToken token, object value = null)
        {
            reader.Read();
            Assert.Equal(token, reader.CurrentToken);
            Assert.Equal(value, reader.CurrentValue);
        }

        private LuaTableTextReader CreateReader(string content)
        {
            return new LuaTableTextReader(new StringReader(content.Replace("\r\n", "\n"))) { CloseReader = true };
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("true   -- Comment", true)]
        [InlineData("-- Comment\nfalse-- Comment", false)]
        [InlineData("nil", null)]
        [InlineData("0/0", double.NaN)]
        [InlineData("123", 123)]
        [InlineData("-0x45f", -0x45f)]
        [InlineData("'abc'", "abc")]
        [InlineData("\"def\\t\"", "def\t")]
        [InlineData("[[ghi\\t\\]]", "ghi\\t\\")]
        public void ReadLiteralTest(string expr, object value)
        {
            using (var reader = CreateReader(expr))
            {
                Assert.Equal(LuaTableReaderToken.None, reader.CurrentToken);
                Assert.Null(reader.CurrentValue);
                AssertNextToken(reader, LuaTableReaderToken.Value, value);
                AssertNextToken(reader, LuaTableReaderToken.None);
                AssertNextToken(reader, LuaTableReaderToken.None);
            }
        }

        [Theory]
        [InlineData("true false")]
        [InlineData("true -- Comment\n123")]
        [InlineData("true,")]
        public void ReadExtraContentTest(string expr)
        {
            using (var reader = CreateReader(expr))
            {
                reader.Read();
                Assert.Throws<LuaTableReaderException>(() => reader.Read());
            }
        }

        [Fact]
        public void ReadTableTest()
        {
            using (var reader = CreateReader(@"
{
    Va = 10,    -- variable A
    b = ""20""      --variable b
    , --[===[Other variables
        Comment content]=]] ]===] test test
    _variable_c1 = 30,
    [""variable_d1""] = 5e15,
    [100] = .15,


LongText1 = [=[  
Space before first LF.
]]test]===]test]=],

LongText2 = [[
First LF should be trimmed.
]],

data = { 10, 20, 30 }, {},
}"))
            {
                reader.PreserveComments = true;
                AssertNextToken(reader, LuaTableReaderToken.TableStart);
                AssertNextToken(reader, LuaTableReaderToken.Key, "Va");
                AssertNextToken(reader, LuaTableReaderToken.Value, 10);
                AssertNextToken(reader, LuaTableReaderToken.Comment, "-- variable A");
                AssertNextToken(reader, LuaTableReaderToken.Key, "b");
                AssertNextToken(reader, LuaTableReaderToken.Value, "20");
                AssertNextToken(reader, LuaTableReaderToken.Comment, "--variable b");
                AssertNextToken(reader, LuaTableReaderToken.Comment, "--[===[Other variables\n        Comment content]=]] ]===] test test");
                AssertNextToken(reader, LuaTableReaderToken.Key, "_variable_c1");
                AssertNextToken(reader, LuaTableReaderToken.Value, 30);
                AssertNextToken(reader, LuaTableReaderToken.Key, "variable_d1");
                AssertNextToken(reader, LuaTableReaderToken.Value, 5e15);
                AssertNextToken(reader, LuaTableReaderToken.Key, 100);
                AssertNextToken(reader, LuaTableReaderToken.Value, .15);
                AssertNextToken(reader, LuaTableReaderToken.Key, "LongText1");
                AssertNextToken(reader, LuaTableReaderToken.Value, "  \nSpace before first LF.\n]]test]===]test");
                AssertNextToken(reader, LuaTableReaderToken.Key, "LongText2");
                AssertNextToken(reader, LuaTableReaderToken.Value, "First LF should be trimmed.\n");
                AssertNextToken(reader, LuaTableReaderToken.Key, "data");
                AssertNextToken(reader, LuaTableReaderToken.TableStart);
                AssertNextToken(reader, LuaTableReaderToken.Value, 10);
                AssertNextToken(reader, LuaTableReaderToken.Value, 20);
                AssertNextToken(reader, LuaTableReaderToken.Value, 30);
                AssertNextToken(reader, LuaTableReaderToken.TableEnd);
                AssertNextToken(reader, LuaTableReaderToken.TableStart);
                AssertNextToken(reader, LuaTableReaderToken.TableEnd);
                AssertNextToken(reader, LuaTableReaderToken.TableEnd);
                AssertNextToken(reader, LuaTableReaderToken.None);
            }
        }

    }
}
