namespace Hik.Samples.Scs.IrcChat.Arguments
{
    /// <summary>
    /// Represents state of a chat user.
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// User if online and available for incoming messages.
        /// </summary>
        Available,

        /// <summary>
        /// User is busy and may not answer to messages.
        /// </summary>
        Busy,

        /// <summary>
        /// User is out.
        /// </summary>
        Out
    }
}
