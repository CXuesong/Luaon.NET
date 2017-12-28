using System;
using System.ComponentModel.Design;
using System.IO;

namespace Luaon
{
    /// <summary>
    /// Provides forward-only writing functionality for LUA tables.
    /// </summary>
    public class LuaTableTextWriter
    {

        public LuaTableTextWriter(TextWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        protected TextWriter Writer { get; }

        public virtual void WriteStartTable()
        {
            Writer.Write('{');
        }

        public virtual void WriteEndTable()
        {
            Writer.Write('}');
        }

        public virtual void WriteStartKey()
        {
            Writer.Write('[');
        }

        public virtual void WriteEndKey()
        {
            Writer.Write(']');
            Writer.Write('=');
        }

        public virtual void WriteKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            WriteStartKey();
            LuaConvert.WriteEscapedString(key, StringDelimiterInfo.DoubleQuote, Writer);
            WriteEndKey();
        }

        public virtual void WriteKey(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key is string s)
            {
                WriteKey(s);
                return;
            }
            var expr = LuaConvert.ToString(key);
            WriteStartKey();
            Writer.Write(expr);
            WriteStartKey();
        }

        public virtual void WriteValue(byte value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(sbyte value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(short value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(ushort value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(int value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(uint value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(long value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(ulong value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(decimal value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(float value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(TimeSpan value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(DateTime value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteValue(DateTimeOffset value)
        {
            Writer.Write(LuaConvert.ToString(value));
        }

    }
}
