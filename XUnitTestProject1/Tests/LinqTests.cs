using System;
using System.Collections.Generic;
using System.Text;
using Luaon.Linq;
using Xunit;
using Xunit.Abstractions;

namespace XUnitTestProject1.Tests
{
    public class LinqTests : TestsBase
    {

        /// <inheritdoc />
        public LinqTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void LTableTest()
        {
            var table = new LTable
            {
                {"Test1", 1},
                2,
                {"Test2", 3},
                4.5,
                {20, 5},
                {true, "6"},
                {"Child", new LTable(1, 2, 3, 4, 5)}
            };
            Assert.Equal(7, table.Count);
            Assert.Equal(1, table["Test1"]);
            Assert.Equal(2, table[1]);
            Assert.Equal(3, table["Test2"]);
            Assert.Equal(4.5, table[2]);
            Assert.Equal(5, table[20]);
            Assert.Equal("6", table[true]);
            Assert.Equal(1, (int)table["Test1"]);
            Assert.Equal(2, (int)table[1]);
            Assert.Equal(3, (int)table["Test2"]);
            Assert.Equal(4, (int)table[2]);
            Assert.Equal("5", (string)table[20]);
            Assert.Equal(6, (int)table[true]);
            var childTable = (LTable)table["Child"];
            Assert.Equal(5, childTable.Count);
            Assert.Equal(1, table["Child"][1]);
            Assert.Equal(2, table["Child"][2]);
            Assert.Equal(3, table["Child"][3]);
            var s = table.ToString();
            Output.WriteLine(s);
        }


    }
}
