using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace Luaon
{

    /// <summary>
    /// Represents type of tokens read by <see cref="LuaTableTextReader"/>.
    /// </summary>
    public enum LuaTableReaderToken
    {
        /// <summary>Either not started, or end of input met.</summary>
        None,

        /// <summary>Start of LUA table expression.</summary>
        TableStart,

        /// <summary>End of LUA table expression.</summary>
        TableEnd,

        /// <summary>LUA table key expression.</summary>
        Key,

        /// <summary>LUA value expression.</summary>
        Value,

        /// <summary>LUA comment.</summary>
        Comment,
    }

    /// <summary>
    /// Provides forward-only reading functionality for Lua tables.
    /// </summary>
    public class LuaTableTextReader : IDisposable
    {

        private readonly List<LuaContainerContext> contextStack;
        private LuaContainerContext currentContext;
        private readonly char[] buffer;
        private int bufferLength;   // Loaded buffer end position
        private int bufferPos;      // Start of next buffer character
        private bool readerEof;

        private static readonly object boxedTrue = true;
        private static readonly object boxedFalse = false;
        private static readonly object boxedZero = 0;

        #region State Management

        private enum State
        {
            Unknown = 0, // Determined by context (e.g. When leaving the current scope)
            Start = 1,   // outside of table
            FieldStart,  // At the middle of the table, but immediately after a field separator
            KeyStart,    // At the beginning of table key expression []
            Key,         // In table key expression, after the key literal
            KeyEnd,      // Immediately after a key expression
            ValueStart,  // Immediately after key expression and an equal sign
            FieldEnd,    // (Should) immediately before a field separator
            End,         // (Should be) end of input
            Error
        }

        private enum Token
        {
            TableStart, // {
            TableEnd,   // }
            KeyStart,   // [
            KeyEnd,     // ]
            Equal,      // =
            Comma,      // ,
            Literal,    // "abc"
        }

        // NextState[State][Token]
        private static readonly State[][] NextStateTable =
        {
            // @formatter:off
            /*                          TableStart        TableEnd        KeyStart         KeyEnd           Equal              Comma              Literal  */
            /* Unknown      */ new[] {State.Error,      State.Error,    State.Error,    State.Error,    State.Error,        State.Error,        State.Error},
            /* Start        */ new[] {State.FieldStart, State.Error,    State.Error,    State.Error,    State.Error,        State.Error,        State.End},
            /* FieldStart   */ new[] {State.FieldStart, State.Unknown,  State.KeyStart, State.Error,    State.Error,        State.Error,        State.FieldEnd},
            /* KeyStart     */ new[] {State.FieldStart, State.Error,    State.Error,    State.Error,    State.Error,        State.Error,        State.Key},
            /* Key          */ new[] {State.Error,      State.Error,    State.Error,    State.KeyEnd,   State.Error,        State.Error,        State.Error},
            /* KeyEnd       */ new[] {State.Error,      State.Error,    State.Error,    State.Error,    State.ValueStart,   State.Error,        State.FieldStart},
            /* ValueStart   */ new[] {State.FieldStart, State.Error,    State.Error,    State.Error,    State.Error,        State.Error,        State.FieldEnd},
            /* FieldEnd     */ new[] {State.Error,      State.Unknown,  State.Error,    State.Error,    State.Error,        State.FieldStart,   State.Error},
            /* End          */ new[] {State.Error,      State.Error,    State.Error,    State.Error,    State.Error,        State.Error,        State.Error},
            /* Error        */ new[] {State.Error,      State.Error,    State.Error,    State.Error,    State.Error,        State.Error,        State.Error},
            // @formatter:on
        };

        private State currentState = State.Start;

        private void GotoNextState(Token token)
        {
            var next = NextStateTable[(int)currentState][(int)token];
            Debug.WriteLine("GotoNextState: {0}[{1}] -> {2}", currentState, token, next);
            if (next == State.Error)
            {
                if (currentState == State.End)
                    throw MakeReaderException("Detected extra content after the end of LUA object.");
                throw MakeReaderException($"Unexpected token: {token}.");
            }
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
                    currentState = State.End;
                    break;
                case LuaContainerType.Table:
                    currentState = context.ContainerType == LuaContainerType.Key
                        ? State.KeyEnd
                        : State.FieldEnd;
                    break;
                case LuaContainerType.Key:
                    currentState = State.Key;
                    break;
                default:
                    Debug.Assert(false, "Invalid ContainerType.");
                    break;
            }
            // Return the popped container type
            return context.ContainerType;
        }

        public string CurrentPath => LuaContainerContext.ToString(contextStack, currentContext);

        private void AssertContainerType(LuaContainerType type)
        {
            if (Peek() != type)
                throw new LuaTableWriterException("Invalid writer state. Expect container type:" + type, CurrentPath);
        }

        #endregion

        public LuaTableTextReader(TextReader reader)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            currentContext = new LuaContainerContext(LuaContainerType.None);
            contextStack = new List<LuaContainerContext>();
            CloseReader = true;
            buffer = new char[1024];
            bufferPos = 0;
            readerEof = false;
        }

        /// <summary>
        /// Gets the underlying reader.
        /// </summary>
        public TextReader Reader { get; }

        /// <summary>
        /// Gets/sets a value that determines whether to close the underlying reader upon disposal.
        /// </summary>
        public bool CloseReader { get; set; }

        /// <summary>
        /// Get/sets a value that determines whether to preserve comments when reading the input.
        /// </summary>
        /// <value>
        /// Set to <c>true</c> to receive <see cref="LuaTableReaderToken.Comment"/> tokens and get the comment
        /// value from <see cref="CurrentValue"/>. Otherwise such token will be skipped.
        /// </value>
        /// <remarks>The default value is <c>false</c>.</remarks>
        public bool PreserveComments { get; set; }

        /// <summary>
        /// Gets the token type that has just been read.
        /// </summary>
        public LuaTableReaderToken CurrentToken { get; private set; }

        /// <summary>
        /// Gets the token value that has just been read.
        /// </summary>
        public object CurrentValue { get; private set; }

        protected bool IsClosed { get; private set; }

        #region Buffer Management

        private bool EnsureBuffer(int length = 0)
        {
            Debug.Assert(length < buffer.Length);
            if (length == 0)
            {
                if (bufferPos < bufferLength)
                    return true;
                bufferPos = 0;
                bufferLength = 0;
                bufferLength = Reader.Read(buffer, 0, buffer.Length);
                if (bufferLength == 0)
                {
                    readerEof = true;
                    return false;
                }
                return true;
            }
            var charsNeeded = length - (bufferLength - bufferPos);
            if (charsNeeded <= 0)
                return true;
            if (readerEof)
                return false;
            if (bufferLength + charsNeeded > buffer.Length)
            {
                // Shift unread chars to the left; reduce buffer length.
                Array.Copy(buffer, bufferPos, buffer, 0, bufferLength - bufferPos);
                bufferLength -= bufferPos;
                bufferPos = 0;
            }
            // Read from where the current loaded buffer ends.
            var charsRead = Reader.ReadBlock(buffer, bufferLength, charsNeeded);
            bufferLength += charsRead;
            if (charsRead < charsNeeded)
            {
                // Reader must have reached EOF.
                Debug.Assert(Reader.Peek() < 0);
                readerEof = true;
                return false;
            }
            return true;
        }

        private bool LookAhead(char c)
        {
            if (EnsureBuffer(1))
                return buffer[bufferPos] == c;
            return false;
        }

        private bool LookAhead(string s, bool caseInsensitive = false)
        {
            Debug.Assert(!string.IsNullOrEmpty(s));
            if (EnsureBuffer(s.Length))
            {
                for (int i = 0; i < s.Length; i++)
                {
                    int x = s[i];
                    int y = buffer[bufferPos + i];
                    if (x != y)
                    {
                        if (caseInsensitive)
                        {
                            if (x >= 'A' && x <= 'Z')
                                x += 'a' - 'A';
                            if (y >= 'A' && y <= 'Z')
                                y += 'a' - 'A';
                            if (x != y)
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private int LookAhead()
        {
            if (EnsureBuffer(1))
            {
                return buffer[bufferPos];
            }
            return -1;
        }

        private bool Consume(char c)
        {
            if (LookAhead(c))
            {
                bufferPos++;
                return true;
            }
            return false;
        }

        private bool Consume(string s, bool caseInsensitive = false)
        {
            if (LookAhead(s, caseInsensitive))
            {
                bufferPos += s.Length;
                return true;
            }
            return false;
        }

        private int Consume()
        {
            if (EnsureBuffer(1))
            {
                var c = buffer[bufferPos];
                bufferPos++;
                return c;
            }
            return -1;
        }

        private void ConsumeWhitespace(bool singleLine)
        {
            while (EnsureBuffer())
            {
                while (CharsLeft > 0)
                {
                    if (singleLine && (buffer[bufferPos] == '\r' || buffer[bufferPos] == '\n') ||
                        !char.IsWhiteSpace(buffer[bufferPos]))
                        return;
                    bufferPos++;
                }
            }
        }

        private int ConsumeUntil(char c1, char c2, StringBuilder collector)
        {
            while (!BufferEof)
            {
                if (CharsLeft == 0)
                    EnsureBuffer();
                var terminatorPos = -1;
                if (c2 == char.MinValue)
                {
                    terminatorPos = Array.IndexOf(buffer, c1, bufferPos, bufferLength - bufferPos);
                }
                else
                {
                    for (int i = bufferPos; i < bufferLength; i++)
                    {
                        if (buffer[i] == c1 || buffer[i] == c2)
                        {
                            terminatorPos = i;
                            break;
                        }
                    }
                }
                if (terminatorPos >= 0)
                {
                    collector?.Append(buffer, bufferPos, terminatorPos - bufferPos);
                    bufferPos = terminatorPos;
                    return buffer[terminatorPos];
                }
                collector?.Append(buffer, bufferPos, bufferLength - bufferPos);
                bufferPos = bufferLength;
            }
            return -1;
        }

        private bool BufferEof => bufferPos == bufferLength && readerEof;

        private int CharsLeft => bufferLength - bufferPos;

        #endregion

        /// <summary>
        /// Advance the reader, reading for the next token.
        /// </summary>
        /// <returns>The token type that has just been read.</returns>
        public LuaTableReaderToken Read()
        {
        START:
            ConsumeWhitespace(false);
            var c = LookAhead();
            if (c < 0)
            {
                CurrentValue = null;
                return CurrentToken = LuaTableReaderToken.None;
            }
            object o;
            if ((o = ReadComment()) != null)
            {
                if (!PreserveComments) goto START;
                CurrentValue = o;
                return CurrentToken = LuaTableReaderToken.Comment;
            }
            switch (c)
            {
                case '{':
                    GotoNextState(Token.TableStart);
                    Push(LuaContainerType.Table);
                    Consume();
                    CurrentValue = null;
                    return CurrentToken = LuaTableReaderToken.TableStart;
                case '[':
                    // "[[" starts a string literal, "[" starts a key expression.
                    if (LookAhead("[[") || LookAhead("[="))
                        break;
                    AssertContainerType(LuaContainerType.Table);
                    GotoNextState(Token.KeyStart);
                    Push(LuaContainerType.Key);
                    Consume();
                    goto START;
                case '}':
                    AssertContainerType(LuaContainerType.Table);
                    GotoNextState(Token.TableEnd);
                    Pop();
                    Consume();
                    CurrentValue = null;
                    return CurrentToken = LuaTableReaderToken.TableEnd;
                case ']':
                    AssertContainerType(LuaContainerType.Key);
                    GotoNextState(Token.KeyEnd);
                    var key = currentContext.BoxedKey;
                    Pop();
                    currentContext.BoxedKey = key;
                    Consume();
                    CurrentValue = key == NilPlaceholder.Instance ? null : key;
                    return CurrentToken = LuaTableReaderToken.Key;
                case '=':
                    GotoNextState(Token.Equal);
                    Consume();
                    goto START;
                case ',':
                    GotoNextState(Token.Comma);
                    Consume();
                    goto START;
            }
            if ((o = ReadLiteral()) != null)
            {
                GotoNextState(Token.Literal);
                Debug.WriteLine("Literal: {0}", o);
                if (currentState == State.Key)
                {
                    currentContext.BoxedKey = o;
                    goto START;
                }
                else
                {
                    if (currentContext.ContainerType == LuaContainerType.Table)
                        currentContext.CurrentIndex++;
                    CurrentValue = o == NilPlaceholder.Instance ? null : o;
                    return CurrentToken = LuaTableReaderToken.Value;
                }
            }
            if ((o = ReadIdentifier()) != null)
            {
                GotoNextState(Token.KeyStart);
                GotoNextState(Token.Literal);
                GotoNextState(Token.KeyEnd);
                Debug.WriteLine("Identifier: {0}", o);
                CurrentValue = o;
                // We keep using BoxedKey instead of Key in Reader.
                currentContext.BoxedKey = (string)o;
                return CurrentToken = LuaTableReaderToken.Key;
            }
            throw MakeUnexpectedCharacterException(c);
        }

        private string ReadComment()
        {
            if (!Consume("--"))
                return null;
            var equalSigns = -1;
            var bracketLevel = -1;
            if (Consume('['))
            {
                equalSigns = 0;
                while (Consume('='))
                    equalSigns++;
                if (Consume('['))
                    // Read: --[==...==[
                    bracketLevel = equalSigns;
            }
            // Parse / skip comment
            StringBuilder sb = null;
            if (PreserveComments)
            {
                sb = new StringBuilder("--");
            }
            if (bracketLevel < 0)
            {
                // Single-line comment
                // -- Comment
                if (sb != null)
                {
                    // Restore [ and = that we have already read.
                    if (equalSigns >= 0)
                    {
                        sb.Append('[');
                        if (equalSigns > 0)
                            sb.Append('=', equalSigns);
                    }
                }
                ConsumeUntil('\r', '\n', sb);
                Consume();
            }
            else
            {
                // Multi-line comment
                // --[==[ Comment
                // Comment
                // Comment]==] comment
                var bracket = StringDelimiterInfo.FromBracketLevel(bracketLevel);
                sb?.Append(bracket.StartDelimiter);
                while (!BufferEof)
                {
                    ConsumeUntil(']', char.MinValue, sb);
                    if (Consume(bracket.EndDelimiter))
                    {
                        sb?.Append(bracket.EndDelimiter);
                        // right bracket detected
                        // skip current line
                        ConsumeUntil('\r', '\n', sb);
                        Consume();
                        break;
                    }
                    else
                    {
                        // ordinary right bracket.
                        sb?.Append((char)Consume());
                    }
                }
            }
            return sb != null ? sb.ToString() : string.Empty;
        }

        private string ReadStringLiteral()
        {
            int bracketLevel;
            if (Consume('"'))
            {
                bracketLevel = -2;
            }
            else if (Consume('\''))
            {
                bracketLevel = -1;
            }
            else if (Consume('['))
            {
                bracketLevel = 0;
                while (Consume('='))
                    bracketLevel++;
                if (!Consume('['))
                {
                    throw MakeUnexpectedCharacterException(LookAhead(), "Illegal start of string expression.");
                }
            }
            else
            {
                return null;
            }
            var sb = new StringBuilder();
            if (bracketLevel < 0)
            {
                // ' or "
                while (true)
                {
                    // Buffer more characters at one time.
                    if (CharsLeft == 0)
                        EnsureBuffer();
                    var c = Consume();
                    switch (c)
                    {
                        case '\\':
                            switch (c = Consume())
                            {
                                case '\\':
                                    sb.Append('\\');
                                    break;
                                case '\'' when bracketLevel == -1:
                                    sb.Append('\'');
                                    break;
                                case '"' when bracketLevel == -2:
                                    sb.Append('"');
                                    break;
                                case 'a':
                                    sb.Append('\a');
                                    break;
                                case 'b':
                                    sb.Append('\b');
                                    break;
                                case 'f':
                                    sb.Append('\f');
                                    break;
                                case 'n':
                                    sb.Append('\n');
                                    break;
                                case 'r':
                                    sb.Append('\r');
                                    break;
                                case 't':
                                    sb.Append('\t');
                                    break;
                                case 'v':
                                    sb.Append('\v');
                                    break;
                                case '[':
                                    sb.Append('[');
                                    break;
                                case ']':
                                    sb.Append(']');
                                    break;
                                case var d1 when d1 >= '0' && d1 <= '9':
                                    // \ddd
                                    d1 -= '0';
                                    var d2 = LookAhead();
                                    int d3;
                                    if (d2 >= '0' && d2 <= '9')
                                    {
                                        d2 -= '0';
                                        Consume();
                                        d3 = LookAhead();
                                        if (d3 >= '0' && d3 <= '9')
                                            d3 -= '0';
                                        else
                                            d3 = -1;
                                    }
                                    else
                                    {
                                        d2 = -1;
                                        d3 = -1;
                                    }
                                    var ch = d1;
                                    if (d2 >= 0) ch = ch * 10 + d2;
                                    if (d3 >= 0) ch = ch * 10 + d3;
                                    if (ch > 0xFF)
                                        throw MakeReaderException(
                                            $"Illegal string escape sequence: \\{ch}. Code point overflow.");
                                    sb.Append((char)ch);
                                    break;
                                default:
                                    if (c < 0)
                                        throw MakeUnexpectedEndOfInputException();
                                    throw MakeReaderException($"Illegal string escape sequence: \\{(char)c}.");
                            }
                            break;
                        case '\'' when bracketLevel == -1:
                        case '"' when bracketLevel == -2:
                            goto EXIT_LOOP;
                        case var c_ when c_ < 0:
                            throw new FormatException("Unexpected end of input.");
                        default:
                            sb.Append((char)c);
                            break;
                    }
                }
            EXIT_LOOP:;
            }
            else
            {
                // [=====[
                // Skip first immediate line ending: \r, \n, \r\n
                Consume('\r');
                Consume('\n');
                while (true)
                {
                    // Buffer more characters at one time.
                    if (CharsLeft == 0)
                        EnsureBuffer();
                    var c = Consume();
                    if (c == ']')
                    {
                        var equalSigns = 0;
                        while ((c = Consume()) == '=')
                            equalSigns++;
                        // End of string detected.
                        if (c == ']' && equalSigns == bracketLevel)
                            break;
                        if (c < 0)
                            throw MakeUnexpectedEndOfInputException();
                        // ... or it's only plain literal text.
                        sb.Append(']');
                        if (equalSigns > 0) sb.Append('=', equalSigns);
                    }
                    sb.Append((char)c);
                }
            }
            return sb.ToString();
        }

        private object ReadNumberLiteral()
        {
            var signFactor = Consume('-') ? -1 : 1;
            // Lua allows something like "-     123".
            ConsumeWhitespace(true);
            var sb = new StringBuilder(32);
            if (Consume("0x", true))
            {
            NEXT_HEX_DIGIT:
                var c = LookAhead();
                if (c >= '0' && c <= '9' || c >= 'A' && c <= 'F' || c >= 'a' && c <= 'f')
                {
                    sb.Append((char)c);
                    Consume();
                    goto NEXT_HEX_DIGIT;
                }
                var expr = sb.ToString();
                try
                {
                    // 0x00000000
                    if (expr.Length < 8)
                    {
                        var v = Convert.ToInt32(expr, 16) * signFactor;
                        return v == 0 ? boxedZero : v;
                    }
                    else
                    {
                        var v = Convert.ToInt64(expr, 16) * signFactor;
                        return v == 0 ? boxedZero : v;
                    }
                }
                catch (Exception ex)
                {
                    throw MakeReaderException($"Cannot represent numeric literal “{expr}” with a proper number type.", ex);
                }
            }
            else
            {
                var c = LookAhead();
                var needFloat = false;
                if (c >= '0' && c <= '9' || c == '.')
                {
                NEXT_DEC_DIGIT:
                    c = LookAhead();
                    if (c >= '0' && c <= '9' || c == 'E' || c == 'e' || c == '.' || c == '+' || c == '-')
                    {
                        if (!needFloat && (c == 'E' || c == 'e' || c == '.'))
                            needFloat = true;
                        sb.Append((char)c);
                        Consume();
                        goto NEXT_DEC_DIGIT;
                    }
                    var expr = sb.ToString();
                    // 2147483647
                    if (!needFloat && expr.Length < 10)
                    {
                        var v = Convert.ToInt32(expr, CultureInfo.InvariantCulture) * signFactor;
                        return v == 0 ? boxedZero : v;
                    }
                    // 9223372036854775807
                    else if (!needFloat && expr.Length < 19)
                    {
                        var v = Convert.ToInt64(expr, CultureInfo.InvariantCulture) * signFactor;
                        return v == 0 ? boxedZero : v;
                    }
                    else
                    {
                        return Convert.ToDouble(expr, CultureInfo.InvariantCulture) * signFactor;
                    }
                }
                else if (signFactor == -1) // We have already consumed "-"
                {
                    throw MakeUnexpectedCharacterException(c);
                }
                else
                {
                    return null;
                }
            }
        }

        private object ReadLiteral()
        {
            if (Consume("nil"))
                return NilPlaceholder.Instance;
            if (Consume("true"))
                return boxedTrue;
            if (Consume("false"))
                return boxedFalse;
            if (Consume("math.huge"))
                return double.PositiveInfinity;
            if (Consume("-math.huge"))
                return double.NegativeInfinity;
            if (Consume("0/0"))
                return double.NaN;
            return ReadStringLiteral() ?? ReadNumberLiteral();
        }

        private string ReadIdentifier()
        {
            var c = LookAhead();
            if (!(c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z'))
                return null;
            var sb = new StringBuilder();
        NEXT_CHAR:
            c = LookAhead();
            if (!(c == '_' || c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z' || c >= '0' && c <= '9'))
            {
                var id = sb.ToString();
                return id;
            }
            Consume();
            sb.Append((char)c);
            goto NEXT_CHAR;
        }

        private LuaTableReaderException MakeReaderException(string message, Exception inner = null)
        {
            return new LuaTableReaderException(message, CurrentPath, inner);
        }

        private LuaTableReaderException MakeUnexpectedCharacterException(int c, string message = null)
        {
            if (c < 0)
                return MakeUnexpectedEndOfInputException();
            return MakeUnexpectedCharacterException((char)c, message);
        }

        private LuaTableReaderException MakeUnexpectedCharacterException(char c, string message = null)
        {
            if (c == '\r' || c == '\n')
                return MakeReaderException("Unexpected end of line. " + message);
            return MakeReaderException($"Unexpected character: “{c}”. " + message);
        }

        private LuaTableReaderException MakeUnexpectedEndOfInputException()
        {
            return MakeReaderException("Unexpected end of input.");
        }

        protected virtual void Close(bool disposing)
        {
            if (IsClosed) return;
            if (disposing)
            {
                if (CloseReader) Reader.Dispose();
            }
            IsClosed = true;
        }

        /// <summary>
        /// Closes the reader.
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

        private class NilPlaceholder
        {

            public static readonly NilPlaceholder Instance = new NilPlaceholder();

            private NilPlaceholder()
            {

            }

            public override string ToString()
            {
                return "nil";
            }

        }

    }
}
