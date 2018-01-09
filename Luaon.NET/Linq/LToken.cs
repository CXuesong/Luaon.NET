using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Luaon.Linq
{
    public abstract class LToken
    {

        /// <summary>
        /// Gets the token node type.
        /// </summary>
        public abstract LTokenType TokenType { get; }

        /// <summary>
        /// Writes this token to a <see cref="LuaTableTextWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="LuaTableTextWriter"/> into which this method will write.</param>
        public abstract void WriteTo(LuaTableTextWriter writer);

        /// <summary>
        /// Makes a recursive deep-clone of the current token.
        /// </summary>
        public abstract LToken DeepClone();

        /// <summary>
        /// Gets the Lua expression string with the specified formatting options.
        /// </summary>
        /// <param name="formatting">The formatting options.</param>
        /// <returns>Lua expression.</returns>
        public string ToString(Formatting formatting)
        {
            using (var writer = new StringWriter())
            {
                using (var lw = new LuaTableTextWriter(writer) { CloseWriter = false, Formatting = formatting})
                {
                    WriteTo(lw);
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Gets the prettified Lua expression string.
        /// </summary>
        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(Formatting.Prettified);
        }

        #region Conversions

        public static explicit operator double(LToken value)
        {
            return (double)(LValue)value;
        }

        public static explicit operator double?(LToken value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) return null;
            return (double)(LValue)value;
        }

        public static explicit operator float(LToken value)
        {
            return (float)(double)(LValue)value;
        }

        public static explicit operator float?(LToken value)
        {
            return (float?)(double?)value;
        }

        public static explicit operator long(LToken value)
        {
            return (long)(LValue)value;
        }

        public static explicit operator long?(LToken value)
        {
            if (value.TokenType == LTokenType.Nil) return null;
            return (long)(LValue)value;
        }

        public static explicit operator int(LToken value)
        {
            return (int)(long)value;
        }

        public static explicit operator int?(LToken value)
        {
            return (int?)(long?)value;
        }

        public static explicit operator bool(LToken value)
        {
            return (bool)(LValue)value;
        }

        public static explicit operator bool?(LToken value)
        {
            if (value.TokenType == LTokenType.Nil) return null;
            return (bool)value;
        }

        public static explicit operator string(LToken value)
        {
            return (string)(LValue)value;
        }

        public static implicit operator LToken(bool value)
        {
            return new LValue(value);
        }

        public static implicit operator LToken(int value)
        {
            return new LValue(value);
        }

        public static implicit operator LToken(long value)
        {
            return new LValue(value);
        }

        public static implicit operator LToken(double value)
        {
            return new LValue(value);
        }

        public static implicit operator LToken(string value)
        {
            return new LValue(value);
        }

        #endregion

        /// <summary>
        /// Gets/sets the child token associated with the specified key.
        /// </summary>
        /// <param name="name">The key of the field to set or get.</param>
        /// <returns>The value of the specified field, or <c>null</c> if the field does not exist.</returns>
        public virtual LToken this[int name]
        {
            get => throw new NotSupportedException($"Cannot access child token of {GetType()}.");
            set => throw new NotSupportedException($"Cannot access child token of {GetType()}.");
        }

        /// <summary>
        /// Gets/sets the child token associated with the specified key.
        /// </summary>
        /// <param name="name">The key of the field to set or get.</param>
        /// <returns>The value of the specified field, or <c>null</c> if the field does not exist.</returns>
        public virtual LToken this[string name]
        {
            get => throw new NotSupportedException($"Cannot access child token of {GetType()}.");
            set => throw new NotSupportedException($"Cannot access child token of {GetType()}.");
        }

        /// <summary>
        /// Gets/sets the child token associated with the specified key.
        /// </summary>
        /// <param name="name">The key of the field to set or get.</param>
        /// <returns>The value of the specified field, or <c>null</c> if the field does not exist.</returns>
        public virtual LToken this[LValue name]
        {
            get => throw new NotSupportedException($"Cannot access child token of {GetType()}.");
            set => throw new NotSupportedException($"Cannot access child token of {GetType()}.");
        }

    }

    public enum LTokenType
    {
        Nil = 0,
        Boolean,
        Integer,
        Float,
        String,
        Table,
        Field,
        Comment,
    }

}
