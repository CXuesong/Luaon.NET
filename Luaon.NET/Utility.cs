using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Luaon
{
    public static class Utility
    {

        public static StringWriter CreateStringWriter(int minCapacity = 0)
        {
            return new StringWriter(new StringBuilder(Math.Max(minCapacity, 16)), CultureInfo.InvariantCulture);
        }

        public static Type GetUnderlyingType(Type t)
        {
            if (t.IsEnum) return Enum.GetUnderlyingType(t);
            var ut = Nullable.GetUnderlyingType(t);
            if (ut != null) return ut;
            return t;
        }

    }
}
