using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;

namespace XUnitTestProject1
{
    public class TestsBase
    {

        public TestsBase(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }
        
    }
}
