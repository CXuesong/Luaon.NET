using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
#if NETSTANDARD2_0
using System.Runtime.Serialization;
#endif

namespace Luaon
{
    /// <summary>
    /// Base type of Luaon.NET exceptions.
    /// </summary>
#if NETSTANDARD2_0
    [Serializable]
#endif
    public class LuaonException : Exception
    {

        public LuaonException()
        {
        }

        public LuaonException(string message) : base(message)
        {
        }

        public LuaonException(string message, Exception inner) : base(message, inner)
        {
        }

#if NETSTANDARD2_0
        [SecurityCritical]
        protected LuaonException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif
    }

#if NETSTANDARD2_0
    [Serializable]
#endif
    public class LuaTableWriterException : LuaonException
    {

        public LuaTableWriterException()
        {
        }

        public LuaTableWriterException(string message) : base(message)
        {
        }

        public LuaTableWriterException(string message, string path) : base(message)
        {
            Path = path;
        }

        public LuaTableWriterException(string message, Exception inner) : base(message, inner)
        {
        }

#if NETSTANDARD2_0
        [SecurityCritical]
        protected LuaTableWriterException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            Path = info.GetString("Path");
        }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Path", Path);
        }
#endif

        /// <summary>
        /// Gets the Lua property path where the exception happens.
        /// </summary>
        public virtual string Path { get; }

        /// <inheritdoc />
        public override string Message
        {
            get
            {
                if (!string.IsNullOrEmpty(Path))
                    return base.Message + "\nPath: " + Path;
                return base.Message;
            }
        }
    }
}
