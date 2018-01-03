using System;
using System.Collections.Generic;
using System.Text;

namespace Luaon
{
    /// <summary>
    /// Specifies formatting options for <see cref="LuaTableTextWriter"/>
    /// </summary>
    [Flags]
    public enum Formatting
    {

        /// <summary>
        /// Compact Lua code formatting.
        /// </summary>
        None = 0,

        /// <summary>
        /// Prettified Lua code formatting.
        /// </summary>
        Prettified = 1,
    }
}
