using System.Collections.Generic;
using Hik.Communication.Scs.Communication.Messages;

namespace Hik.Communication.Scs.Communication.Protocols
{
    /// <summary>
    /// Represents a byte-level communication protocol between applications.
    /// </summary>
    public interface IScsWireProtocol
    {
        /// <summary>
        /// Serializes a message to a byte array to send to remote application.
        /// This method is synchronized. So, only one thread can call it concurrently.
        /// </summary>
        /// <param name="message">Message to be serialized</param>
        byte[] GetBytes(IScsMessage message);

        /// <summary>
        /// Builds messages from a byte array that is received from remote application.
        /// The Byte array may contain just a part of a message, the protocol must
        /// cumulate bytes to build messages.
        /// This method is synchronized. So, only one thread can call it concurrently.
        /// </summary>
        /// <param name="receivedBytes">Received bytes from remote application</param>
        /// <returns>
        /// List of messages.
        /// Protocol can generate more than one message from a byte array.
        /// Also, if received bytes are not sufficient to build a message, the protocol
        /// may return an empty list (and save bytes to combine with next method call).
        /// </returns>
        IEnumerable<IScsMessage> CreateMessages(byte[] receivedBytes);

        /// <summary>
        /// This method is called when connection with remote application is reset (connection is renewing or first connecting).
        /// So, wire protocol must reset itself.
        /// </summary>
        void Reset();
    }
}
