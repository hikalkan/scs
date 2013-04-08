using Hik.Communication.ScsServices.Service;
using Hik.Samples.Scs.IrcChat.Arguments;

namespace Hik.Samples.Scs.IrcChat.Contracts
{
    /// <summary>
    /// This interface defines methods of chat client.
    /// Defined methods are called by chat server.
    /// </summary>
    public interface IChatClient
    {
        /// <summary>
        /// This method is used to get user list from chat server.
        /// It is called by server once after user logged in to server.
        /// </summary>
        /// <param name="users">All online user informations</param>
        void GetUserList(UserInfo[] users);

        /// <summary>
        /// This method is called from chat server to inform that a message
        /// is sent to chat room publicly.
        /// </summary>
        /// <param name="nick">Nick of sender</param>
        /// <param name="message">Message</param>
        void OnMessageToRoom(string nick, ChatMessage message);

        /// <summary>
        /// This method is called from chat server to inform that a message
        /// is sent to the current user privately.
        /// </summary>
        /// <param name="nick">Nick of sender</param>
        /// <param name="message">Message</param>
        void OnPrivateMessage(string nick, ChatMessage message);

        /// <summary>
        /// This method is called from chat server to inform that a new user
        /// joined to chat room.
        /// </summary>
        /// <param name="userInfo">Informations of new user</param>
        void OnUserLogin(UserInfo userInfo);

        /// <summary>
        /// This method is called from chat server to inform that an existing user
        /// has left the chat room.
        /// </summary>
        /// <param name="nick">Informations of new user</param>
        void OnUserLogout(string nick);

        /// <summary>
        /// This method is called from chat server to inform that a user changed his/her status.
        /// </summary>
        /// <param name="nick">Nick of the user</param>
        /// <param name="newStatus">New status of the user</param>
        void OnUserStatusChange(string nick, UserStatus newStatus);
    }
}
