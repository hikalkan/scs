using System;
using System.Runtime.Serialization;

namespace Hik.Samples.Scs.IrcChat.Exceptions
{
    /// <summary>
    /// This exception is thrown by Chat server if a user wants to login
    /// with a nick that is being used by another user.
    /// </summary>
    [Serializable]
    public class NickInUseException : ApplicationException
    {
        /// <summary>
        /// Contstructor.
        /// </summary>
        public NickInUseException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        public NickInUseException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public NickInUseException(string message)
            : base(message)
        {

        }
    }
}
