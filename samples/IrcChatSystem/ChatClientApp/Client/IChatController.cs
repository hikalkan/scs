using Hik.Samples.Scs.IrcChat.Arguments;

namespace Hik.Samples.Scs.IrcChat.Client
{
    /// <summary>
    /// This interface defines method of chat controller that can be used by views.
    /// </summary>
    public interface IChatController
    {
        /// <summary>
        /// Connects to the server.
        /// It automatically Logins to server if connection success.
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnects from server if it is connected.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends a public message to room.
        /// It will be seen all users in room.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        void SendMessageToRoom(ChatMessage message);

        /// <summary>
        /// Change status of user.
        /// </summary>
        /// <param name="newStatus">New status</param>
        void ChangeStatus(UserStatus newStatus);

        /// <summary>
        /// Sends a private message to a user.
        /// </summary>
        /// <param name="nick">Destination nick</param>
        /// <param name="message">Message</param>
        void SendPrivateMessage(string nick, ChatMessage message);
    }
}