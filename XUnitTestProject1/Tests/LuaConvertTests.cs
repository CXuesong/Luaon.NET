using System;
using Luaon;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTestProject1.Tests
{
    public class LuaConvertTests : TestsBase
    {
        /// <inheritdoc />
        public LuaConvertTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ToStringTest()
        {
            Assert.Equal(@"""test \\ \a\b\r\n\""abc""", LuaConvert.ToString("test \\ \a\b\r\n\"abc"));
            Assert.Equal(@"'abc""def'", LuaConvert.ToString("abc\"def", "'"));
            Assert.Equal(@"[[test]]", LuaConvert.ToString("test", "[["));
            Assert.Equal(@"[==[test]==]", LuaConvert.ToString("test", "[==["));
            Assert.Equal(@"[==========[test]==========]", LuaConvert.ToString("test", "[==========["));
            Assert.Equal("-12345", LuaConvert.ToString(-12345));
            Assert.Equal("123.45", LuaConvert.ToString(123.45));
            Assert.Equal(@"""http://cxuesong.com/""", LuaConvert.ToString(new Uri("http://cxuesong.com/")));
            Assert.Equal("nil", LuaConvert.ToString((object)null));
            Assert.Equal("-12345", LuaConvert.ToString((object)-12345));
            Assert.Equal("123.45", LuaConvert.ToString((object)123.45));
        }

    }
}
