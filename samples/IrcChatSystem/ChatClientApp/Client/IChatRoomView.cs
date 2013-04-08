using Hik.Samples.Scs.IrcChat.Arguments;

namespace Hik.Samples.Scs.IrcChat.Client
{
    /// <summary>
    /// This interface is used to interact with main chat room window by ChatController.
    /// It is implemented by main window.
    /// </summary>
    public interface IChatRoomView
    {
        /// <summary>
        /// This method is called when a message is sent to chat room.
        /// </summary>
        /// <param name="nick">Nick of sender</param>
        /// <param name="message">Message</param>
        void OnMessageReceived(string nick, ChatMessage message);

        /// <summary>
        /// This method is called when a private message is sent to the current user.
        /// </summary>
        /// <param name="nick">Nick of sender</param>
        /// <param name="message">The message</param>
        void OnPrivateMessageReceived(string nick, ChatMessage message);

        /// <summary>
        /// This method is called when user successfully logged in to chat server.
        /// </summary>
        void OnLoggedIn();

        /// <summary>
        /// This method is used to inform view if login is failed.
        /// </summary>
        /// <param name="errorMessage">Detail of error</param>
        void OnLoginError(string errorMessage);

        /// <summary>
        /// This method is called when connection to server is closed.
        /// </summary>
        void OnLoggedOut();

        /// <summary>
        /// This methos is used to add a new user to user list in room view.
        /// </summary>
        /// <param name="userInfo">Informations of new user</param>
        void AddUserToList(UserInfo userInfo);

        /// <summary>
        /// This metrhod is used to remove a user (that is disconnected from server) from user list in room view.
        /// </summary>
        /// <param name="nick">Nick of user to remove</param>
        void RemoveUserFromList(string nick);

        /// <summary>
        /// This method is called from chat server to inform that a user changed his/her status.
        /// </summary>
        /// <param name="nick">Nick of the user</param>
        /// <param name="newStatus">New status of the user</param>
        void OnUserStatusChange(string nick, UserStatus newStatus);
    }
}