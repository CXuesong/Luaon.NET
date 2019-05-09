using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace Luaon.Json
{
    /// <summary>
    /// A writer that accepts JSON tokens but outputs Lua table expression.
    /// </summary>
    public class JsonLuaWriter : JsonWriter
    {

        private readonly TextWriter writer;
        private int _MaxUnquotedNameLength = 64;
        private int _Indentation = 2;
        private char _IndentChar;
        private int nestedLevel = 0;
        private char[] currentIdentation;
        private string currentIdentationNewLine;

        public JsonLuaWriter(TextWriter writer)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _Indentation = 2;
            _IndentChar = ' ';
        }

        /// <summary>
        /// Gets/sets the maximum allowed table field name that will be checked for chance of
        /// using <c>Name = </c> expression instad of <c>["key"] = </c>, to reduce the
        /// generated code length and improve redability.
        /// </summary>
        /// <value>A positive value, or <c>0</c> to disable this feature.</value>
        /// <remarks>The default value is <c>64</c>.</remarks>
        public int MaxUnquotedNameLength
        {
            get { return _MaxUnquotedNameLength; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _MaxUnquotedNameLength = value;
            }
        }

        /// <summary>
        /// Gets or sets how many <see cref="IndentChar"/>s to write for each level in the hierarchy
        /// when <see cref="Formatting"/> is set to <see cref="Formatting.Prettified"/>.
        /// </summary>
        public int Indentation
        {
            get { return _Indentation; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
                if (_Indentation != value)
                {
                    _Indentation = value;
                    currentIdentation = null;
                }
            }
        }

        /// <summary>
        /// Gets/sets which character to use for indenting
        /// when <see cref="Formatting"/> is set to <see cref="Formatting.Prettified"/>.
        /// </summary>
        public char IndentChar
        {
            get { return _IndentChar; }
            set
            {
                if (_Indentation != value)
                {
                    _IndentChar = value;
                    currentIdentation = null;
                }
            }
        }

        /// <inheritdoc />
        public override void WriteStartArray()
        {
            base.WriteStartArray();
            nestedLevel++;
            writer.Write('{');
        }

        /// <inheritdoc />
        public override void WriteEndArray()
        {
            nestedLevel--;
            Debug.Assert(nestedLevel >= 0);
            base.WriteEndArray();
        }

        /// <inheritdoc />
        public override void WriteStartObject()
        {
            base.WriteStartObject();
            nestedLevel++;
            writer.Write('{');
        }

        /// <inheritdoc />
        public override void WriteEndObject()
        {
            nestedLevel--;
            Debug.Assert(nestedLevel >= 0);
            base.WriteEndObject();
        }

        /// <inheritdoc />
        public override void WriteStartConstructor(string name)
        {
            throw new NotSupportedException();
            //base.WriteStartConstructor(name);
        }

        /// <inheritdoc />
        public override void WriteEndConstructor()
        {
            throw new NotSupportedException();
            //base.WriteEndConstructor();
        }

        /// <inheritdoc />
        protected override void WriteEnd(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    writer.Write('}');
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public override void WritePropertyName(string name)
        {
            base.WritePropertyName(name);
            if (name.Length > 0 && name.Length < _MaxUnquotedNameLength && LuaConvert.IsValidIdentifier(name))
            {
                writer.Write(name);
            }
            else
            {
                writer.Write('[');
                writer.Write(LuaConvert.ToString(name));
                writer.Write(']');
            }

            if (Formatting == Newtonsoft.Json.Formatting.Indented)
                writer.Write(" =");     // base class will insert a space after =
            else
                writer.Write('=');
        }

        /// <inheritdoc />
        protected override void WriteIndent()
        {
            var indentCount = nestedLevel * Indentation;
            if (currentIdentation == null || currentIdentation.Length < currentIdentationNewLine.Length + indentCount)
            {
                var newLine = writer.NewLine;
                var chars = new char[newLine.Length + indentCount];
                newLine.CopyTo(0, chars, 0, newLine.Length);
                for (int i = newLine.Length; i < chars.Length; i++) chars[i] = _IndentChar;
                currentIdentation = chars;
                currentIdentationNewLine = newLine;
            }

            writer.Write(currentIdentation, 0, currentIdentationNewLine.Length + indentCount);
        }

        /// <inheritdoc />
        protected override void WriteValueDelimiter()
        {
            writer.Write(',');
        }

        /// <inheritdoc />
        protected override void WriteIndentSpace()
        {
            writer.Write(' ');
        }

        #region WriteValue methods

        /// <inheritdoc />
        public override void WriteNull()
        {
            base.WriteNull();
            writer.Write(LuaConvert.Nil);
        }

        /// <inheritdoc />
        public override void WriteUndefined()
        {
            throw new NotSupportedException();
            //base.WriteUndefined();
        }

        /// <inheritdoc />
        public override void WriteRaw(string json)
        {
            // Actually, this should be Lua
            base.WriteRaw(json);
            writer.Write(json);
        }

        /// <inheritdoc />
        public override void WriteValue(string value)
        {
            base.WriteValue(value);
            if (value == null)
                writer.Write(LuaConvert.Nil);
            else
                LuaConvert.WriteString(writer, value);
        }

        /// <inheritdoc />
        public override void WriteValue(bool value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(char value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(byte value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(short value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(ushort value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(int value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(uint value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(long value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(ulong value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(float value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(double value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(DateTime value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(DateTimeOffset value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(byte[] value)
        {
            base.WriteValue(value);
            writer.Write('"');
            writer.Write(Convert.ToBase64String(value));
            writer.Write('"');
        }

        /// <inheritdoc />
        public override void WriteValue(Guid value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value.ToString("D")));
        }

        /// <inheritdoc />
        public override void WriteValue(TimeSpan value)
        {
            base.WriteValue(value);
            writer.Write(LuaConvert.ToString(value));
        }

        /// <inheritdoc />
        public override void WriteValue(Uri value)
        {
            base.WriteValue(value);
            if (value == null)
                writer.Write(LuaConvert.Nil);
            else
                writer.Write(LuaConvert.ToString(value));
        }

        #endregion

        /// <inheritdoc />
        public override void Flush()
        {
            writer.Flush();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (CloseOutput) writer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
