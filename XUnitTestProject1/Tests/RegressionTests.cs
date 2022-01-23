using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Luaon;
using Luaon.Linq;
using Xunit;

namespace XUnitTestProject1.Tests
{
    public class RegressionTests
    {

        /// <summary>
        /// UnexpectedCharacterException when parsing long Lua table (> LuaTableTextReader buffer size, i.e., 1024).
        /// </summary>
        [Theory]
		[InlineData("0123456789ABCDEF", 3)]
		[InlineData("0123456789ABCDEF", 10)]
        [InlineData("0123456789ABCDEF", 64)]
        [InlineData("0123456789ABCDEF", 100)]
        public void Issue1(string seed, int repeat)
        {
            var bigTable = new LTable
            {
                ["Field1"] = 12345,
                ["Field2"] = "Test",
            };
            for (var i = 0; i < repeat; i++)
                bigTable[i] = seed + i;
            var serialized = bigTable.ToString(Formatting.Prettified);
            Assert.Contains("12345", serialized);
            Assert.Contains("Test", serialized);
            var bigTable2 = LToken.Parse(serialized);
            Assert.Equal(bigTable.ToString(), bigTable2.ToString());
        }

        /// <summary>
        /// LUA table key being truncated by reader buffer boundary.
        /// </summary>
        [Theory]
        [InlineData("0123456789ABCDEF", 3)]
        [InlineData("0123456789ABCDEF", 10)]
        [InlineData("0123456789ABCDEF", 64)]
        [InlineData("0123456789ABCDEF", 100)]
        public void Issue3(string seed, int repeat)
        {
            var bigTable = new LTable
            {
                ["Field1"] = 12345,
                ["Field2"] = "Test",
            };
            for (var i = 0; i < repeat; i++)
                bigTable["TestField" + i] = seed + i;
            var serialized = bigTable.ToString(Formatting.Prettified);
            Assert.Contains("12345", serialized);
            Assert.Contains("Test", serialized);
            var bigTable2 = LToken.Parse(serialized);
            Assert.Equal(bigTable.ToString(), bigTable2.ToString());
        }

    }
}
