using Hik.Communication.ScsServices.Service;
using Hik.Samples.Scs.IrcChat.Arguments;

namespace Hik.Samples.Scs.IrcChat.Contracts
{
    /// <summary>
    /// This interface defines Chat Service Contract.
    /// It is used by Chat clients to interact with Chat Server.
    /// </summary>
    [ScsService(Version = "1.0.0.0")]
    public interface IChatService
    {
        /// <summary>
        /// Used to login to chat service.
        /// </summary>
        /// <param name="userInfo">User informations</param>
        void Login(UserInfo userInfo);
        
        /// <summary>
        /// Sends a public message to room.
        /// It will be seen by all users in room.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        void SendMessageToRoom(ChatMessage message);
        
        /// <summary>
        /// Sends a private message to a specific user.
        /// Message will be seen only by destination user.
        /// </summary>
        /// <param name="destinationNick">Nick of the destination user
        /// who will receive the message</param>
        /// <param name="message">Message to be sent</param>
        void SendPrivateMessage(string destinationNick, ChatMessage message);

        /// <summary>
        /// Changes status of a user and inform all other users.
        /// </summary>
        /// <param name="newStatus">New status of user</param>
        void ChangeStatus(UserStatus newStatus);
        
        /// <summary>
        /// Used to logout from chat service.
        /// Client may not call this method while logging out (in an application crash situation),
        /// it will also be logged out automatically when connection fails between
        /// client and server.
        /// </summary>
        void Logout();
    }
}
