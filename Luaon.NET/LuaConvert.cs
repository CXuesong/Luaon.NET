using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;

namespace Luaon
{
    public static class LuaConvert
    {

        /// <summary>
        /// The Lua representation of <c>true</c>.
        /// </summary>
        public static readonly string True = "true";

        /// <summary>
        /// The Lua representation of <c>false</c>.
        /// </summary>
        public static readonly string False = "false";

        /// <summary>
        /// The Lua representation of <c>nil</c>.
        /// </summary>
        public static readonly string Nil = "nil";

        /// <summary>
        /// The Lua representation of positive infinity.
        /// </summary>
        public static readonly string PositiveInfinity = "math.huge";

        /// <summary>
        /// The Lua representation of negative infinity.
        /// </summary>
        public static readonly string NegativeInfinity = "-math.huge";

        /// <summary>
        /// The Lua representation of nan.
        /// </summary>
        public static readonly string NaN = "0/0";

        /// <summary>
        /// Converts the <see cref="bool"/> value into Lua representation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(bool value)
        {
            return value ? True : False;
        }

        /// <summary>
        /// Converts the nullable <see cref="bool"/> value into Lua representation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(bool? value)
        {
            if (value == null) return Nil;
            return value.Value ? True : False;
        }

        /// <summary>Converts the <see cref="char"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(char value)
        {
            return ToString(value.ToString());
        }

        /// <summary>Converts the nullable <see cref="char"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(char? value)
        {
            if (value == null) return Nil;
            return ToString(value.ToString());
        }

        /// <summary>Converts the <see cref="byte"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(byte value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="byte"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(byte? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="sbyte"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(sbyte value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="sbyte"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(sbyte? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="short"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(short value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="short"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(short? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="ushort"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(ushort value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="ushort"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(ushort? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="int"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="int"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(int? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="uint"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(uint value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="uint"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(uint? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="long"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="long"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(long? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="ulong"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(ulong value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="ulong"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(ulong? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="decimal"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="decimal"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(decimal? value)
        {
            if (value == null) return Nil;
            return value.Value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the <see cref="float"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(float value)
        {
            if (float.IsNaN(value)) return NaN;
            if (float.IsPositiveInfinity(value)) return PositiveInfinity;
            if (float.IsNegativeInfinity(value)) return NegativeInfinity;
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="float"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(float? value)
        {
            if (value == null) return Nil;
            return ToString(value.Value);
        }

        /// <summary>Converts the <see cref="float"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(double value)
        {
            if (double.IsNaN(value)) return NaN;
            if (double.IsPositiveInfinity(value)) return PositiveInfinity;
            if (double.IsNegativeInfinity(value)) return NegativeInfinity;
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts the nullable <see cref="double"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(double? value)
        {
            if (value == null) return Nil;
            return ToString(value.Value);
        }

        /// <summary>
        /// Converts the <see cref="string"/> value into Lua expression.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <remarks>
        /// This overload uses <c>"</c> as value delimiter.
        /// </remarks>
        public static string ToString(string value)
        {
            return ToString(value, "\"");
        }

        /// <summary>Converts the <see cref="Uri"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(Uri value)
        {
            if (value == null) return Nil;
            return ToString(value.ToString());
        }

        /// <summary>Converts the <see cref="TimeSpan"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(TimeSpan value)
        {
            return ToString(value.ToString());
        }

        /// <summary>Converts the nullable <see cref="TimeSpan"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(TimeSpan? value)
        {
            if (value == null) return Nil;
            return ToString(value.Value.ToString());
        }

        /// <summary>Converts the <see cref="DateTime"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(DateTime value)
        {
            return ToString(value.ToString("O"));
        }

        /// <summary>Converts the nullable <see cref="DateTime"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(DateTime? value)
        {
            if (value == null) return Nil;
            return ToString(value.Value.ToString("O"));
        }

        /// <summary>Converts the <see cref="DateTimeOffset"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(DateTimeOffset value)
        {
            return ToString(value.ToString("O"));
        }

        /// <summary>Converts the nullable <see cref="DateTimeOffset"/> value into Lua expression.</summary>
        /// <param name="value">The value to convert.</param>
        public static string ToString(DateTimeOffset? value)
        {
            if (value == null) return Nil;
            return ToString(value.Value.ToString("O"));
        }

        /// <summary>
        /// Converts the <see cref="string"/> value into Lua expression.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="delimiter">The delimiter used to start the Lua string expression.</param>
        /// <remarks>
        /// Lua supports starting a string expression with <c>'</c>, <c>"</c>, or <c>[====[</c>, where in
        /// the last case, there can be any (including 0) count of equal signs.
        /// </remarks>
        public static string ToString(string value, string delimiter)
        {
            var delm = StringDelimiterInfo.FromStartDelimiter(delimiter);
            using (var writer = Utility.CreateStringWriter((value?.Length ?? 0) + delimiter.Length * 2))
            {
                WriteEscapedString(value, delm, writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Converts any simple value into Lua expression.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <remarks>The value passed in should be suiltable for any other of <c>ToString</c> overloads.</remarks>
        public static string ToString(object value)
        {
            if (value == null) return Nil;
            var ut = Utility.GetUnderlyingType(value.GetType());
            if (ut == typeof(char)) return ToString((char)value);
            if (ut == typeof(bool)) return ToString((bool)value);
            if (ut == typeof(sbyte)) return ToString((sbyte)value);
            if (ut == typeof(short)) return ToString((short)value);
            if (ut == typeof(ushort)) return ToString((ushort)value);
            if (ut == typeof(int)) return ToString((int)value);
            if (ut == typeof(byte)) return ToString((byte)value);
            if (ut == typeof(uint)) return ToString((uint)value);
            if (ut == typeof(long)) return ToString((long)value);
            if (ut == typeof(ulong)) return ToString((ulong)value);
            if (ut == typeof(float)) return ToString((float)value);
            if (ut == typeof(double)) return ToString((double)value);
            if (ut == typeof(string)) return ToString((string)value);
            if (ut == typeof(Uri)) return ToString((Uri)value);
            if (ut == typeof(TimeSpan)) return ToString((TimeSpan)value);
            if (ut == typeof(DateTime)) return ToString((DateTime)value);
            if (ut == typeof(DateTimeOffset)) return ToString((DateTimeOffset)value);
            throw new ArgumentException($"Unsupported type: {ut}.");
        }

        internal static void WriteEscapedString(string value, StringDelimiterInfo delimiter, TextWriter writer)
        {
            Debug.Assert(delimiter != null);
            if (string.IsNullOrEmpty(value))
            {
                writer.Write(delimiter.StartDelimiter);
                writer.Write(delimiter.EndDelimiter);
                return;
            }
            if (delimiter.IsBracket)
            {
                if (value[0] == '\n')
                    throw new InvalidOperationException(@"String value cannot start with \n when using bracket as delimiter.");
                if (value.Contains(delimiter.EndDelimiter))
                    throw new InvalidOperationException(@"String value cannot contain closing long bracket sequence of the proper level.");
                writer.Write(delimiter.StartDelimiter);
                writer.Write(value);
                writer.Write(delimiter.EndDelimiter);
            }
            else
            {
                writer.Write(delimiter.StartDelimiter);
                foreach (var c in value)
                {
                    switch (c)
                    {
                        case '\a':
                            writer.Write("\\a");
                            break;
                        case '\b':
                            writer.Write("\\b");
                            break;
                        case '\f':
                            writer.Write("\\f");
                            break;
                        case '\n':
                            writer.Write("\\n");
                            break;
                        case '\r':
                            writer.Write("\\r");
                            break;
                        case '\t':
                            writer.Write("\\t");
                            break;
                        case '\v':
                            writer.Write("\\v");
                            break;
                        case '\"' when delimiter.IsDoubleQuote:
                            writer.Write("\\\"");
                            break;
                        case '\'' when delimiter.IsSingleQuote:
                            writer.Write("\\\'");
                            break;
                        default:
                            writer.Write(c);
                            break;
                    }
                }
                writer.Write(delimiter.EndDelimiter);
            }
        }

    }

    internal sealed class StringDelimiterInfo : IEquatable<StringDelimiterInfo>
    {

        private readonly int data;

        public static readonly StringDelimiterInfo SingleQuote = new StringDelimiterInfo(-1, "'", "'");
        public static readonly StringDelimiterInfo DoubleQuote = new StringDelimiterInfo(-2, "\"", "\"");
        public static readonly StringDelimiterInfo Brackets0 = new StringDelimiterInfo(0, "[[", "]]");
        public static readonly StringDelimiterInfo Brackets1 = new StringDelimiterInfo(0, "[=[", "]=]");
        public static readonly StringDelimiterInfo Brackets2 = new StringDelimiterInfo(0, "[==[", "]==]");
        public static readonly StringDelimiterInfo Brackets3 = new StringDelimiterInfo(0, "[===[", "]===]");
        public static readonly StringDelimiterInfo Brackets4 = new StringDelimiterInfo(0, "[====[", "]====]");

        public static StringDelimiterInfo FromStartDelimiter(string delimiter)
        {
            if (delimiter == null) throw new ArgumentNullException(nameof(delimiter));
            if (delimiter.Length == 0) goto INVALID_DELIMITER;
            switch (delimiter)
            {
                case "'": return new StringDelimiterInfo(-1, "'", "'");
                case "\"": return new StringDelimiterInfo(-2, "\"", "\"");
                case "[[": return new StringDelimiterInfo(0, "[[", "]]");
                case "[=[": return new StringDelimiterInfo(1, "[=[", "]=]");
                case "[==[": return new StringDelimiterInfo(2, "[==[", "]==]");
                case "[===[": return new StringDelimiterInfo(3, "[===[", "]===]");
                case "[====[": return new StringDelimiterInfo(4, "[====[", "]====]");
                case "[=====[": return new StringDelimiterInfo(5, "[=====[", "]=====]");
                case "[======[": return new StringDelimiterInfo(6, "[======[", "]======]");
                case "[=======[": return new StringDelimiterInfo(7, "[=======[", "]=======]");
                case "[========[": return new StringDelimiterInfo(8, "[========[", "]========]");
                default:
                    if (delimiter[0] != '[') goto INVALID_DELIMITER;
                    if (delimiter[delimiter.Length - 1] != '[') goto INVALID_DELIMITER;
                    for (int i = 1; i < delimiter.Length - 1; i++) if (delimiter[i] != '=') goto INVALID_DELIMITER;
                    var sb = new StringBuilder("]", delimiter.Length);
                    sb.Append('=', delimiter.Length - 2);
                    sb.Append(']');
                    return new StringDelimiterInfo(delimiter.Length - 2, delimiter, sb.ToString());
            }
            INVALID_DELIMITER:
            throw new ArgumentException("Invalid string delimiter.", nameof(delimiter));
        }

        private StringDelimiterInfo(int data, string startDelimiter, string endDelimiter)
        {
            Debug.Assert(startDelimiter != null);
            Debug.Assert(endDelimiter != null);
            this.data = data;
            StartDelimiter = startDelimiter;
            EndDelimiter = endDelimiter;
        }

        public string StartDelimiter { get; }

        public string EndDelimiter { get; }

        public bool IsSingleQuote => data == -1;

        public bool IsDoubleQuote => data == -2;

        public bool IsBracket => data >= 0;

        public int BracketLevel => data;

        /// <inheritdoc />
        public bool Equals(StringDelimiterInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return data == other.data;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is StringDelimiterInfo info && Equals(info);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return data;
        }

        public static bool operator ==(StringDelimiterInfo left, StringDelimiterInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(StringDelimiterInfo left, StringDelimiterInfo right)
        {
            return !Equals(left, right);
        }
    }

}
