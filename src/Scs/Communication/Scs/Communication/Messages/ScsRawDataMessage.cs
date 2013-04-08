using System;

namespace Hik.Communication.Scs.Communication.Messages
{
    /// <summary>
    /// This message is used to send/receive a raw byte array as message data.
    /// </summary>
    [Serializable]
    public class ScsRawDataMessage : ScsMessage
    {
        /// <summary>
        /// Message data that is being transmitted.
        /// </summary>
        public byte[] MessageData { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ScsRawDataMessage()
        {
            
        }

        /// <summary>
        /// Creates a new ScsRawDataMessage object with MessageData property.
        /// </summary>
        /// <param name="messageData">Message data that is being transmitted</param>
        public ScsRawDataMessage(byte[] messageData)
        {
            MessageData = messageData;
        }

                /// <summary>
        /// Creates a new reply ScsRawDataMessage object with MessageData property.
        /// </summary>
        /// <param name="messageData">Message data that is being transmitted</param>
        /// <param name="repliedMessageId">
        /// Replied message id if this is a reply for
        /// a message.
        /// </param>
        public ScsRawDataMessage(byte[] messageData, string repliedMessageId)
            : this(messageData)
        {
            RepliedMessageId = repliedMessageId;
        }

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            var messageLength = MessageData == null ? 0 : MessageData.Length;
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? string.Format("ScsRawDataMessage [{0}]: {1} bytes", MessageId, messageLength)
                       : string.Format("ScsRawDataMessage [{0}] Replied To [{1}]: {2} bytes", MessageId, RepliedMessageId, messageLength);
        }
    }
}
