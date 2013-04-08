using System;

namespace Hik.Samples.Scs.IrcChat.Arguments
{
    /// <summary>
    /// Represents a chat user.
    /// This object particularly used in Login of a user.
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// Nick of user.
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// Bytes of avatar of user.
        /// </summary>
        public byte[] AvatarBytes { get; set; }

        /// <summary>
        /// Status of user.
        /// </summary>
        public UserStatus Status { get; set; }
    }
}
