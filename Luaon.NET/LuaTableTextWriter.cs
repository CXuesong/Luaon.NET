using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Luaon
{
    /// <summary>
    /// Provides forward-only writing functionality for Lua tables.
    /// </summary>
    public class LuaTableTextWriter : IDisposable
    {

        private enum State
        {
            Unknown = 0,    // Determined by context (e.g. When leaving the current sope)
            Start = 1,      // outside of table
            TableStart,     // At the beginning of the table {}
            FieldStart,     // At the middle of the table, but immediately after a field separator
            KeyStart,       // At the beginning of table key expression []
            Key,            // In table key expression
            KeyEnd,         // Immediately after a key expression
            Error
        }

        private enum Token
        {
            TableStart,     // {
            TableEnd,       // }
            KeyStart,       // [
            KeyEnd,         // ] = 
            Literal,        // "abc"
        }

        private Formatting _Formatting;
        private readonly List<LuaContainerContext> contextStack;
        private LuaContainerContext currentContext;
        private State currentState = State.Start;
        private int _Indentation;
        private int _MaxUnquotedNameLength = 64;
        private char _IndentChar;
        private char[] currentIdentation;
        private string currentIdentationNewLine;

        // NextState[State][Token]
        private static readonly State[][] NextStateTable =
        {
            /*                          TableStart        TableEnd        KeyStart         KeyEnd       Literal*/
            /* Unknown      */ new[] {State.Error,      State.Error,    State.Error,    State.Error,    State.Error},
            /* Start        */ new[] {State.TableStart, State.Error,    State.Error,    State.Error,    State.Start},
            /* TableStart   */ new[] {State.TableStart, State.Unknown,  State.KeyStart, State.Error,    State.FieldStart},
            /* FieldStart   */ new[] {State.TableStart, State.Unknown,  State.KeyStart, State.Error,    State.FieldStart},
            /* KeyStart     */ new[] {State.TableStart, State.Error,    State.Error,    State.Error,    State.Key},
            /* Key          */ new[] {State.Error,      State.Error,    State.Error,    State.KeyEnd,   State.Error},
            /* KeyEnd       */ new[] {State.TableStart, State.Error,    State.Error,    State.Error,    State.FieldStart},
            /* Error        */ new[] {State.Error,      State.Error,    State.Error,    State.Error,    State.Error},
        };

        public LuaTableTextWriter(TextWriter writer)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            currentContext = new LuaContainerContext(LuaContainerType.None);
            contextStack = new List<LuaContainerContext>();
            CloseWriter = true;
            Indentation = 2;
            IndentChar = ' ';
        }

        /// <summary>
        /// Gets the underlying writer.
        /// </summary>
        public TextWriter Writer { get; }

        /// <summary>
        /// Gets/sets a value that determines whether to close the underlying writer unpon disposal.
        /// </summary>
        public bool CloseWriter { get; set; }

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

        protected bool IsClosed { get; private set; }

        public Formatting Formatting
        {
            get { return _Formatting; }
            set
            {
                if ((value & (Formatting.Prettified)) != value)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _Formatting = value;
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
                    currentIdentation = null;
                    _Indentation = value;
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
                if (_IndentChar != value)
                {
                    currentIdentation = null;
                    _IndentChar = value;
                }
            }
        }

        private void GotoNextState(Token token)
        {
            var next = NextStateTable[(int)currentState][(int)token];
            if (next == State.Error)
                throw new LuaTableWriterException("Cannot write the specified token while keeping the table expression valid.", CurrentPath);
            currentState = next;
        }

        private void Push(LuaContainerType type)
        {
            contextStack.Add(currentContext);
            currentContext = new LuaContainerContext(type);
        }

        private LuaContainerType Peek()
        {
            return currentContext.ContainerType;
        }

        private LuaContainerType Pop()
        {
            var context = currentContext;
            currentContext = contextStack[contextStack.Count - 1];
            contextStack.RemoveAt(contextStack.Count - 1);
            // Determine outer container type
            switch (currentContext.ContainerType)
            {
                case LuaContainerType.None:
                    Debug.Assert(contextStack.Count == 0);
                    currentState = State.Start;
                    break;
                case LuaContainerType.Table:
                    currentState = context.ContainerType == LuaContainerType.Key
                        ? State.KeyEnd
                        : State.FieldStart;
                    break;
                case LuaContainerType.Key:
                    currentState = State.Key;
                    break;
                default:
#if NETSTANDARD2_0
                    Debug.Fail("Invalid ContainerType.");
#else
                    Debug.Assert(false);
#endif
                    break;
            }
            // Return the poped container type
            return context.ContainerType;
        }

        internal string CurrentPath => LuaContainerContext.ToString(contextStack, currentContext);

        private void AssertContainerType(LuaContainerType type)
        {
            if (Peek() != type)
                throw new LuaTableWriterException("Invalid writer state. Expect container type:" + type, CurrentPath);
        }

        /// <summary>
        /// Writes the beginning of a Lua table.
        /// </summary>
        public virtual void WriteStartTable()
        {
            GotoNextState(Token.TableStart);
            Push(LuaContainerType.Table);
            Writer.Write('{');
        }

        /// <summary>
        /// Writes the end of a Lua table.
        /// </summary>
        public virtual void WriteEndTable()
        {
            AssertContainerType(LuaContainerType.Table);
            GotoNextState(Token.TableEnd);
            Pop();
            if ((Formatting & Formatting.Prettified) == Formatting.Prettified) WriteIndent();
            Writer.Write('}');
        }

        /// <summary>
        /// Writes the beginning of a Lua table key (property).
        /// </summary>
        public virtual void WriteStartKey()
        {
            AssertContainerType(LuaContainerType.Table);
            DelimitLastValue(Token.KeyStart);
            Writer.Write('[');
            Push(LuaContainerType.Key);
            currentContext.Key = "[]";
            currentContext.KeyIsExpression = true;
        }

        /// <summary>
        /// Writes the end of a Lua table field key.
        /// </summary>
        public virtual void WriteEndKey()
        {
            AssertContainerType(LuaContainerType.Key);
            Pop();
            if ((_Formatting & Formatting.Prettified) == Formatting.Prettified)
                Writer.Write("] = ");
            else
                Writer.Write("]=");
            currentContext.Key = "[...]";
            currentContext.KeyIsExpression = true;
        }

        /// <summary>
        /// Writes a Lua table field key.
        /// </summary>
        /// <param name="key">The key content to write.</param>
        public virtual void WriteKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length <= MaxUnquotedNameLength && LuaConvert.IsValidIdentifier(key))
            {
                AssertContainerType(LuaContainerType.Table);
                DelimitLastValue(Token.KeyStart);
                GotoNextState(Token.Literal);
                Writer.Write(key);
                if ((_Formatting & Formatting.Prettified) == Formatting.Prettified)
                    Writer.Write(" = ");
                else
                    Writer.Write("=");
                GotoNextState(Token.KeyEnd);
            }
            else
            {
                WriteStartKey();
                WriteLiteral(key);
                WriteEndKey();
                currentContext.Key = key;
                currentContext.KeyIsExpression = false;
            }
        }

        /// <summary>
        /// Writes a Lua table field key.
        /// </summary>
        public virtual void WriteKey(int key)
        {
            WriteStartKey();
            WriteLiteral(key);
            WriteEndKey();
            currentContext.Key = key.ToString();
            currentContext.KeyIsExpression = true;
        }

        /// <summary>
        /// Writes a Lua table field key.
        /// </summary>
        public virtual void WriteKey(long key)
        {
            WriteStartKey();
            WriteLiteral(key);
            WriteEndKey();
            currentContext.Key = key.ToString();
            currentContext.KeyIsExpression = true;
        }

        /// <summary>
        /// Writes a Lua table field key.
        /// </summary>
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
            GotoNextState(Token.Literal);
            Writer.Write(expr);
            WriteEndKey();
            currentContext.Key = expr;
            currentContext.KeyIsExpression = true;
        }

        public virtual void WriteFieldDelimiter()
        {
            // Can be , or ;
            Writer.Write(',');
        }

        protected virtual void WriteIndent()
        {
            var indentCount = contextStack.Count * Indentation;
            if (currentIdentation == null || currentIdentation.Length - currentIdentationNewLine.Length < indentCount)
            {
                var newLine = Writer.NewLine;
                var chars = new char[newLine.Length + indentCount];
                newLine.CopyTo(0, chars, 0, newLine.Length);
                for (int i = newLine.Length; i < chars.Length; i++) chars[i] = _IndentChar;
                currentIdentation = chars;
                currentIdentationNewLine = newLine;
            }

            Writer.Write(currentIdentation, 0, currentIdentationNewLine.Length + indentCount);
        }

        private void DelimitLastValue(Token nextToken)
        {
            var s = currentState;
            GotoNextState(nextToken);

            if (s == State.FieldStart)
            {
                // In the middle of a table.
                if (currentContext.Key != null)
                {
                    currentContext.Key = null;
                }
                else
                {
                    currentContext.CurrentIndex++;
                }

                WriteFieldDelimiter();
            }

            if (nextToken != Token.TableEnd && (s == State.TableStart || s == State.FieldStart))
            {
                if ((Formatting & Formatting.Prettified) == Formatting.Prettified) WriteIndent();
            }
        }

        #region WriteLiteral overloads

        public virtual void WriteLiteral(byte value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }
        public virtual void WriteLiteral(byte? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(sbyte value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(sbyte? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(short value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(short? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(ushort value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(ushort? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(int value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(int? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(uint value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(uint? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(long value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(long? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(ulong value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(ulong? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(decimal value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }
        public virtual void WriteLiteral(decimal? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(float value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(float? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(TimeSpan value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(TimeSpan? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(DateTime value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(DateTime? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(DateTimeOffset value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(DateTimeOffset? value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(string value)
        {
            DelimitLastValue(Token.Literal);
            LuaConvert.WriteEscapedString(value, StringDelimiterInfo.DoubleQuote, Writer);
        }

        public virtual void WriteLiteral(Uri value)
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.ToString(value));
        }

        public virtual void WriteLiteral(object value)
        {
            var expr = LuaConvert.ToString(value);
            DelimitLastValue(Token.Literal);
            Writer.Write(expr);
        }

        public virtual void WriteNil()
        {
            DelimitLastValue(Token.Literal);
            Writer.Write(LuaConvert.Nil);
        }

        #endregion

        /// <summary>
        /// Flushes whatever is in the buffer to the destination and also flushes the destination.
        /// </summary>
        public virtual void Flush()
        {
            Writer.Flush();
        }

        protected virtual void Close(bool disposing)
        {
            if (IsClosed) return;
            if (disposing)
            {
                Flush();
                if (CloseWriter) Writer.Dispose();
            }
            IsClosed = true;
        }

        /// <summary>
        /// Closes the writer.
        /// </summary>
        public void Close()
        {
            Close(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            this.Close();
        }
    }
}
