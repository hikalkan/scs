using System;

namespace Hik.Communication.Scs.Communication.Messages
{
    /// <summary>
    /// This message is used to send/receive ping messages.
    /// Ping messages is used to keep connection alive between server and client.
    /// </summary>
    [Serializable]
    public sealed class ScsPingMessage : ScsMessage
    {
        ///<summary>
        /// Creates a new PingMessage object.
        ///</summary>
        public ScsPingMessage()
        {

        }

        /// <summary>
        /// Creates a new reply PingMessage object.
        /// </summary>
        /// <param name="repliedMessageId">
        /// Replied message id if this is a reply for
        /// a message.
        /// </param>
        public ScsPingMessage(string repliedMessageId)
            : this()
        {
            RepliedMessageId = repliedMessageId;
        }

        /// <summary>
        /// Creates a string to represents this object.
        /// </summary>
        /// <returns>A string to represents this object</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(RepliedMessageId)
                       ? string.Format("ScsPingMessage [{0}]", MessageId)
                       : string.Format("ScsPingMessage [{0}] Replied To [{1}]", MessageId, RepliedMessageId);
        }
    }
}
