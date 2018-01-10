using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Luaon.Linq
{
    public class LField : LToken
    {
        private LToken _Value;

        /// <summary>
        /// Initializes a new positional Lua table field with the specified name and value.
        /// </summary>
        /// <inheritdoc cref="LField(LValue,LToken)"/>
        public LField(LToken value) : this(null, value)
        {
        }

        /// <summary>
        /// Initializes a new Lua table field with the specified name and value.
        /// </summary>
        /// <param name="name">Name of the field. Use <c>null</c> for positional field.</param>
        /// <param name="value">Value of the field. Can be <c>null</c>, which means <see cref="LValue.Nil"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="name"/>.<see cref="LValue.TokenType"/> is <see cref="LTokenType.Nil"/>.</exception>
        public LField(LValue name, LToken value)
        {
            if (name != null && name.TokenType == LTokenType.Nil)
                throw new ArgumentException("Cannot use nil as field name.");
            Name = name;
            _Value = value ?? LValue.Nil;
        }

        // Though Lua even allows table expressions as keys, it's pointless in serialization.
        /// <summary>
        /// Gets the name of the Lua table field.
        /// </summary>
        /// <value>The field name, or <c>null</c> for field without explict field name.</value>
        public LValue Name { get; }

        /// <summary>
        /// Gets/sets the value of the Lua table field.
        /// </summary>
        public LToken Value
        {
            get { return _Value; }
            set
            {
                if (value != null && value.TokenType == LTokenType.Field)
                    throw new ArgumentException("Cannot set value to LField instance.");
                _Value = value ?? LValue.Nil;
            }
        }

        /// <inheritdoc />
        public override LTokenType TokenType => LTokenType.Field;

        /// <inheritdoc />
        public override void WriteTo(LuaTableTextWriter writer)
        {
            if (Name != null)
            {
                switch (Name.TokenType)
                {
                    case LTokenType.Boolean:
                        writer.WriteKey((bool)Name);
                        break;
                    case LTokenType.Integer:
                        writer.WriteKey((long)Name);
                        break;
                    case LTokenType.Float:
                        writer.WriteKey((double)Name);
                        break;
                    case LTokenType.String:
                        writer.WriteKey((string)Name);
                        break;
                    default:
#if NETSTANDARD2_0
                        Debug.Fail("Invalid Name.TokenType.");
#else
                        Debug.Assert(false);
#endif
                        break;
                }
            }
            Value.WriteTo(writer);
        }

        /// <inheritdoc />
        public override LToken DeepClone()
        {
            return new LField(Name, Value);
        }
    }
}
