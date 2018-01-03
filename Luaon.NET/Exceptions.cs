using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace Luaon
{
    /// <summary>
    /// Base type of Luaon.NET exceptions.
    /// </summary>
    [Serializable]
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

        [SecurityCritical]
        protected LuaonException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
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
