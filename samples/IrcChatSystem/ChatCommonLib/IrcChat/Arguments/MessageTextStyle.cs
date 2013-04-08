using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hik.Samples.Scs.IrcChat.Arguments
{
    /// <summary>
    /// Represents text style of messages.
    /// </summary>
    [Serializable]
    public class MessageTextStyle
    {
        /// <summary>
        /// True, if message is sent as Bold.
        /// </summary>
        public bool IsBold { get; set; }

        /// <summary>
        /// True, if message is sent as italic.
        /// </summary>
        public bool IsItalic { get; set; }

        /// <summary>
        /// Font family of message.
        /// </summary>
        public string FontFamily { get; set; }

        /// <summary>
        /// Message text color.
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// Size of message text.
        /// </summary>
        public int TextSize { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageTextStyle()
        {
            FontFamily = "Verdana";
            TextColor = new Color {Blue = 255, Green = 255, Red = 255};
            TextSize = 12;
        }

        /// <summary>
        /// Represents a color.
        /// </summary>
        [Serializable]
        public class Color
        {
            /// <summary>
            /// Red value of color.
            /// </summary>
            public byte Red { get; set; }

            /// <summary>
            /// Green value of color.
            /// </summary>
            public byte Green { get; set; }

            /// <summary>
            /// Blue value of color.
            /// </summary>
            public byte Blue { get; set; }
        }
    }
}
