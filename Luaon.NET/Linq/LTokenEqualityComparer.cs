using System;
using System.Collections.Generic;
using System.Text;

namespace Luaon.Linq
{
    /// <summary>
    /// Contains functionality for comparing the deep equality of <see cref="LToken"/> instances.
    /// </summary>
    public class LTokenEqualityComparer : IEqualityComparer<LToken>
    {

        /// <summary>
        /// Gets an instance of <see cref="LTokenEqualityComparer"/>.
        /// </summary>
        public static LTokenEqualityComparer Instance { get; } = new LTokenEqualityComparer();

        /// <inheritdoc />
        public bool Equals(LToken x, LToken y)
        {
            return LToken.DeepEquals(x, y);
        }

        /// <inheritdoc />
        public int GetHashCode(LToken obj)
        {
            if (ReferenceEquals(obj, null)) return 0;
            return obj.GetDeepHashCode();
        }

    }
}
