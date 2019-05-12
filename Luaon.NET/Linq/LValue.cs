using System;
using System.Collections.Generic;
using System.Text;

namespace Luaon.Linq
{
    /// <summary>
    /// Represents a read-only Lua value node.
    /// </summary>
    public class LValue : LToken, IEquatable<LValue>
    {

        private const int INT_VALUE_CHACHE_SIZE = 256;

        private static readonly LValue[] intValueCache = new LValue[INT_VALUE_CHACHE_SIZE];

        /// <summary>
        /// Gets a <see cref="LValue"/> instance representing <c>nil</c> in Lua.
        /// </summary>
        public static LValue Nil { get; } = new LValue();

        /// <summary>
        /// Gets a <see cref="LValue"/> instance representing <c>true</c> in Lua.
        /// </summary>
        public static LValue True { get; } = new LValue(true);

        /// <summary>
        /// Gets a <see cref="LValue"/> instance representing <c>false</c> in Lua.
        /// </summary>
        public static LValue False { get; } = new LValue(false);

        public new static LValue Load(LuaTableTextReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (reader.CurrentToken == LuaTableReaderToken.None)
                reader.Read();
            SkipComments(reader);
            AssertReaderToken(reader, LuaTableReaderToken.Value);
            var v = new LValue(reader.CurrentValue);
            reader.Read();
            return v;
        }

        public LValue(LValue other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            Value = other.Value;
        }

        private LValue()
        {
            Value = null;
            TokenType = LTokenType.Nil;
        }

        #region Conversions

        public LValue(string value)
        {
            TokenType = value == null ? LTokenType.Nil : LTokenType.String;
            this.Value = value;
        }

        public LValue(int value)
        {
            TokenType = LTokenType.Integer;
            this.Value = value;
        }

        public LValue(bool value)
        {
            TokenType = LTokenType.Boolean;
            this.Value = value;
        }

        public LValue(long value)
        {
            TokenType = LTokenType.Integer;
            this.Value = value;
        }

        public LValue(ulong value)
        {
            TokenType = LTokenType.Integer;
            this.Value = value;
        }

        public LValue(float value)
        {
            TokenType = LTokenType.Float;
            this.Value = value;
        }

        public LValue(double value)
        {
            TokenType = LTokenType.Float;
            this.Value = value;
        }

        public LValue(object value)
        {
            this.Value = value;
            switch (value)
            {
                case null:
                    TokenType = LTokenType.Nil;
                    break;
                case bool v:
                    TokenType = LTokenType.Boolean;
                    break;
                case byte v:
                    this.Value = (int) v;
                    TokenType = LTokenType.Integer;
                    break;
                case sbyte v:
                    this.Value = (int) v;
                    TokenType = LTokenType.Integer;
                    break;
                case short v:
                    this.Value = (int) v;
                    TokenType = LTokenType.Integer;
                    break;
                case ushort v:
                    this.Value = (int) v;
                    TokenType = LTokenType.Integer;
                    break;
                case int v:
                    TokenType = LTokenType.Integer;
                    break;
                case uint v:
                    TokenType = LTokenType.Integer;
                    break;
                case long v:
                    TokenType = LTokenType.Integer;
                    break;
                case ulong v:
                    TokenType = LTokenType.Integer;
                    break;
                case float v:
                    TokenType = LTokenType.Float;
                    break;
                case double v:
                    TokenType = LTokenType.Float;
                    break;
                case decimal v:
                    TokenType = LTokenType.Float;
                    break;
                case string v:
                    TokenType = LTokenType.String;
                    break;
                default:
                    throw new ArgumentException("Cannot determine Lua data type for " + value.GetType() + ".");
            }
        }

        public static explicit operator double(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) throw new ArgumentNullException(nameof(value));
            return Convert.ToDouble(value.Value);
        }

        public static explicit operator double?(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) return null;
            return Convert.ToDouble(value.Value);
        }

        public static explicit operator float(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) throw new ArgumentNullException(nameof(value));
            return Convert.ToSingle(value.Value);
        }

        public static explicit operator float?(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) return null;
            return Convert.ToSingle(value.Value);
        }

        public static explicit operator long(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) throw new ArgumentNullException(nameof(value));
            return Convert.ToInt64(value.Value);
        }

        public static explicit operator long?(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) return null;
            return Convert.ToInt64(value.Value);
        }

        public static explicit operator int(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) throw new ArgumentNullException(nameof(value));
            return Convert.ToInt32(value.Value);
        }

        public static explicit operator int?(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) return null;
            return Convert.ToInt32(value.Value);
        }

        public static explicit operator bool(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) throw new ArgumentNullException(nameof(value));
            return Convert.ToBoolean(value.Value);
        }

        public static explicit operator bool?(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) return null;
            return Convert.ToBoolean(value.Value);
        }

        public static explicit operator string(LValue value)
        {
            if (value == null || value.TokenType == LTokenType.Nil) return null;
            return Convert.ToString(value.Value);
        }

        public static implicit operator LValue(bool value)
        {
            return value ? True : False;
        }

        public static implicit operator LValue(int value)
        {
            if (value >= 0 && value < INT_VALUE_CHACHE_SIZE)
            {
                var v = intValueCache[value];
                if (v == null)
                {
                    v = new LValue(value);
                    intValueCache[value] = v;
                }
                return v;
            }
            return new LValue(value);
        }

        public static implicit operator LValue(long value)
        {
            return new LValue(value);
        }

        public static implicit operator LValue(double value)
        {
            return new LValue(value);
        }

        public static implicit operator LValue(string value)
        {
            return new LValue(value);
        }

        #endregion

        /// <inheritdoc />
        public override LTokenType TokenType { get; }

        /// <summary>
        /// Gets the underlying value of this token.
        /// </summary>
        public object Value { get; }

        /// <inheritdoc />
        public override void WriteTo(LuaTableTextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            writer.WriteLiteral(Value);
        }

        /// <inheritdoc />
        public override LToken DeepClone()
        {
            return new LValue(this);
        }

        /// <inheritdoc />
        public bool Equals(LValue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (TokenType != other.TokenType) return false;
            if (Equals(Value, other.Value)) return true;
            if (TokenType == LTokenType.Integer && Value.GetType() != other.Value.GetType())
                return Convert.ToDecimal(Value) == Convert.ToDecimal(other.Value);
            return false;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LValue) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        public static bool operator ==(LValue left, LValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LValue left, LValue right)
        {
            return !Equals(left, right);
        }

    }
}
