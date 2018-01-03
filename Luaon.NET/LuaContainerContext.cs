using System;
using System.Collections.Generic;
using System.Text;

namespace Luaon
{

    internal enum LuaContainerType
    {
        None = 0,
        Table,
        Key
    }

    internal struct LuaContainerContext
    {

        public readonly LuaContainerType ContainerType;

        // Key != null: Named field; Key == null: Indexed field.
        public string Key;

        public bool KeyIsExpression;

        public int CurrentIndex;

        public LuaContainerContext(LuaContainerType containerType)
        {
            ContainerType = containerType;
            Key = null;
            KeyIsExpression = true;
            CurrentIndex = 1;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }

        private void ToString(StringBuilder sb)
        {
            if (Key == null)
            {
                sb.Append('[');
                sb.Append(CurrentIndex);
                sb.Append(']');
            }
            else if (KeyIsExpression)
            {
                sb.Append(Key);
            }
            else if (Key.IndexOfAny(specialChars) != -1)
            {
                sb.Append('.');
                sb.Append(Key);
            }
            else
            {
                sb.Append('[');
                sb.Append(Key);
                sb.Append(']');
            }
        }

        private static readonly char[] specialChars = {' ', '\t', '.', '[', ']', '{', '}'};

        public static string ToString(IEnumerable<LuaContainerContext> stack, LuaContainerContext current)
        {
            var sb = new StringBuilder();
            foreach (var c in stack)
            {
                c.ToString(sb);
            }
            current.ToString(sb);
            return sb.ToString();
        }

    }
}
