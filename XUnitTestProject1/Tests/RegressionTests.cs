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
        [Fact]
        public void Issue1()
        {
            var longString1 = string.Join("", Enumerable.Repeat("0123456789ABCDEF", 64)); // 16 x 64 = 1024
            var longString2 = longString1 + "||" + longString1;
            var bigTable = new LTable
            {
                ["Field1"] = longString1,
                ["Field2"] = 12345,
                ["Field2"] = longString2,
            };
            var serialized = bigTable.ToString(Formatting.Prettified);
            Assert.Contains(longString1, serialized);
            Assert.Contains(longString2, serialized);
            var bigTable2 = LToken.Parse(serialized);
            Assert.Equal(bigTable, bigTable2);
        }

    }
}
