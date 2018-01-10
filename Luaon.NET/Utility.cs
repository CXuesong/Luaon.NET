using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
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
#if NETSTANDARD2_0
            if (t.IsEnum) return Enum.GetUnderlyingType(t);
#else
            if (t.GetTypeInfo().IsEnum) return Enum.GetUnderlyingType(t);
#endif
            var ut = Nullable.GetUnderlyingType(t);
            if (ut != null) return ut;
            return t;
        }

        
        private static readonly byte[] log_2 = new byte[256]
        {
            /* log_2[i] = ceil(log2(i - 1)) */
            0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
            6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8
        };

        // https://github.com/lua/lua/blob/3ad33fde8511af0c771edf20e128588ab86b967d/lobject.c#L65
        /// <summary>
        /// Computes ceil(log2(x)).
        /// </summary>
        public static int CeilLog2(int x)
        {
            Debug.Assert(x > 0);
            int l = 0;
            x--;
            while (x >= 256) { l += 8; x >>= 8; }
            return l + log_2[x];
        }

    }

}
