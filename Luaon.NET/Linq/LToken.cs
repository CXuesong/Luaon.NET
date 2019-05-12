using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Luaon.Linq
{
    /// <summary>
    /// Represents a token in the Lua table expression.
    /// </summary>
    public abstract class LToken
    {

        /// <summary>
        /// Loads next <see cref="LToken"/> from the specified <see cref="LuaTableTextReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to load the next <see cref="LToken"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is <c>null</c>.</exception>
        /// <exception cref="LuaTableReaderException">There are unexpected input in the <paramref name="reader"/>.</exception>
        /// <returns>The loaded Lua token.</returns>
        public static LToken Load(LuaTableTextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (reader.CurrentToken == LuaTableReaderToken.None)
                reader.Read();
            SkipComments(reader);
            switch (reader.CurrentToken)
            {
                case LuaTableReaderToken.None:
                case LuaTableReaderToken.TableStart:
                    return LTable.Load(reader);
                case LuaTableReaderToken.Key:
                    return LField.Load(reader);
                case LuaTableReaderToken.Value:
                    return LValue.Load(reader);
                default:
                    throw MakeUnexpectedTokenException(reader);
            }
        }

        /// <summary>
        /// Parses the <see cref="LToken"/> from the specified Lua expression.
        /// </summary>
        /// <param name="expression">The Lua expression to be parsed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expression"/> is <c>null</c>.</exception>
        /// <exception cref="LuaTableReaderException"><paramref name="expression"/> is empty or invalid; or it contains extra content after the first parsed token.</exception>
        /// <returns>The loaded Lua token.</returns>
        public static LToken Parse(string expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            using (var reader = new StringReader(expression))
            using (var lreader = new LuaTableTextReader(reader))
            {
                var token = Load(lreader);
                SkipComments(lreader);
                if (lreader.CurrentToken != LuaTableReaderToken.None)
                    throw new LuaTableReaderException($"Detected extra content after parsing complete: {lreader.CurrentToken}.",
                        lreader.CurrentPath);
                return token;
            }
        }

        protected static void SkipComments(LuaTableTextReader reader)
        {
            while (reader.CurrentToken == LuaTableReaderToken.Comment)
                reader.Read();
        }

        protected static void AssertReaderToken(LuaTableTextReader reader, LuaTableReaderToken expectedToken)
        {
            if (reader.CurrentToken != expectedToken)
                throw MakeUnexpectedTokenException(reader, expectedToken);
        }

        protected static LuaTableReaderException MakeUnexpectedTokenException(LuaTableTextReader reader,
            LuaTableReaderToken expectedToken = LuaTableReaderToken.None)
        {
            if (reader.CurrentToken == LuaTableReaderToken.None)
                return new LuaTableReaderException("Unexpected end of input.", reader.CurrentPath);
            var message = $"Unexpected Lua token: {reader.CurrentToken}.";
            if (expectedToken != LuaTableReaderToken.None)
                message += $" Expected: {expectedToken}.";
            return new LuaTableReaderException(message, reader.CurrentPath);
        }

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
                using (var lw = new LuaTableTextWriter(writer) { CloseWriter = false, Formatting = formatting })
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
            return (double) (LValue) value;
        }

        public static explicit operator double?(LToken value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) return null;
            return (double) (LValue) value;
        }

        public static explicit operator float(LToken value)
        {
            return (float) (double) (LValue) value;
        }

        public static explicit operator float?(LToken value)
        {
            return (float?) (double?) value;
        }

        public static explicit operator long(LToken value)
        {
            return (long) (LValue) value;
        }

        public static explicit operator long?(LToken value)
        {
            if (value.TokenType == LTokenType.Nil) return null;
            return (long) (LValue) value;
        }

        public static explicit operator int(LToken value)
        {
            return (int) (long) value;
        }

        public static explicit operator int?(LToken value)
        {
            return (int?) (long?) value;
        }

        public static explicit operator bool(LToken value)
        {
            return (bool) (LValue) value;
        }

        public static explicit operator bool?(LToken value)
        {
            if (value.TokenType == LTokenType.Nil) return null;
            return (bool) value;
        }

        public static explicit operator string(LToken value)
        {
            return (string) (LValue) value;
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

        internal abstract int GetDeepHashCode();

        internal abstract bool DeepEquals(LToken other);

        public static bool DeepEquals(LToken x, LToken y)
        {
            if (x == null || y == null) return x == null && y == null;
            return x.DeepEquals(y);
        }

        #region Accessors

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

        #endregion

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
