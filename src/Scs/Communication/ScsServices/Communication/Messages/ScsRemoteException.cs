using System;
using System.Runtime.Serialization;

namespace Hik.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// Represents a SCS Remote Exception.
    /// This exception is used to send an exception from an application to another application.
    /// </summary>
    [Serializable]
    public class ScsRemoteException : Exception
    {
        /// <summary>
        /// Contstructor.
        /// </summary>
        public ScsRemoteException()
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        public ScsRemoteException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
            
        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        public ScsRemoteException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Contstructor.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public ScsRemoteException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
