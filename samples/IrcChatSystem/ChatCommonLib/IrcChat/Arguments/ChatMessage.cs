using System;

namespace Hik.Samples.Scs.IrcChat.Arguments
{
    /// <summary>
    /// Represents a chat message that can be sent and received by chat users.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        /// <summary>
        /// Message text.
        /// </summary>
        public string MessageText { get; set; }

        ///<summary>
        /// Text style of this message.
        ///</summary>
        public MessageTextStyle TextStyle { get; set; }

        /// <summary>
        /// Creates a new ChatMessage.
        /// </summary>
        public ChatMessage()
        {
            TextStyle = new MessageTextStyle();
            MessageText = "";
        }

        /// <summary>
        /// Creates a new ChatMessage.
        /// </summary>
        /// <param name="messageText">Message text</param>
        public ChatMessage(string messageText)
        {
            TextStyle = new MessageTextStyle();
            MessageText = messageText;
        }

        /// <summary>
        /// Creates a new ChatMessage.
        /// </summary>
        /// <param name="messageText">Message text</param>
        /// <param name="textStyle">Text style of this message</param>
        public ChatMessage(string messageText, MessageTextStyle textStyle)
        {
            TextStyle = textStyle;
            MessageText = messageText;
        }
    }
}