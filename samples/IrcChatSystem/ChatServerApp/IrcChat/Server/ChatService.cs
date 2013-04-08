using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hik.Collections;
using Hik.Communication.ScsServices.Service;
using Hik.Samples.Scs.IrcChat.Arguments;
using Hik.Samples.Scs.IrcChat.Contracts;
using Hik.Samples.Scs.IrcChat.Exceptions;

namespace Hik.Samples.Scs.IrcChat.Server
{
    /// <summary>
    /// This class implements Chat Service Contract.
    /// </summary>
    internal class ChatService : ScsService, IChatService
    {
        #region Public Events

        /// <summary>
        /// This event is raised when online user list is changes.
        /// It is usually raised when a new user log in or a user log out.
        /// </summary>
        public event EventHandler UserListChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a list of online users.
        /// </summary>
        public List<UserInfo> UserList
        {
            get
            {
                return (from client in _clients.GetAllItems()
                        select client.User).ToList();
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// List of all connected clients.
        /// </summary>
        private readonly ThreadSafeSortedList<long, ChatClient> _clients;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public ChatService()
        {
            _clients = new ThreadSafeSortedList<long, ChatClient>();
        }

        #endregion

        #region IChatService methods

        /// <summary>
        /// Used to login to chat service.
        /// </summary>
        /// <param name="userInfo">User informations</param>
        public void Login(UserInfo userInfo)
        {
            //Check nick if it is being used by another user
            if (FindClientByNick(userInfo.Nick) != null)
            {
                throw new NickInUseException("The nick '" + userInfo.Nick + "' is being used by another user. Please select another one.");
            }

            //Get a reference to the current client that is calling this method
            var client = CurrentClient;

            //Get a proxy object to call methods of client when needed
            var clientProxy = client.GetClientProxy<IChatClient>();

            //Create a ChatClient and store it in a collection
            var chatClient = new ChatClient(client, clientProxy, userInfo);
            _clients[client.ClientId] = chatClient;

            //Register to Disconnected event to know when user connection is closed
            client.Disconnected += Client_Disconnected;

            //Start a new task to send user list to new user and to inform
            //all users that a new user joined to room
            Task.Factory.StartNew(
                () =>
                {
                    OnUserListChanged();
                    SendUserListToClient(chatClient);
                    SendUserLoginInfoToAllClients(userInfo);
                });
        }

        /// <summary>
        /// Sends a public message to room.
        /// It will be seen all users in room.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        public void SendMessageToRoom(ChatMessage message)
        {
            //Get ChatClient object
            var senderClient = _clients[CurrentClient.ClientId];
            if (senderClient == null)
            {
                throw new ApplicationException("Can not send message before login.");
            }

            //Send message to all online users
            Task.Factory.StartNew(
                () =>
                {
                    foreach (var chatClient in _clients.GetAllItems())
                    {
                        try
                        {
                            chatClient.ClientProxy.OnMessageToRoom(senderClient.User.Nick, message);
                        }
                        catch
                        {

                        }
                    }
                });
        }

        /// <summary>
        /// Sends a private message to a specific user.
        /// Message will be seen only by destination user.
        /// </summary>
        /// <param name="destinationNick">Nick of the destination user who will receive message</param>
        /// <param name="message">Message to be sent</param>
        public void SendPrivateMessage(string destinationNick, ChatMessage message)
        {
            //Get ChatClient object for sender user
            var senderClient = _clients[CurrentClient.ClientId];
            if (senderClient == null)
            {
                throw new ApplicationException("Can not send message before login.");
            }

            //Get ChatClient object for destination user
            var receiverClient = FindClientByNick(destinationNick);
            if (receiverClient == null)
            {
                throw new ApplicationException("There is no online user with nick " + destinationNick);
            }

            //Send message to destination user
            receiverClient.ClientProxy.OnPrivateMessage(senderClient.User.Nick, message);
        }

        /// <summary>
        /// Changes status of a user and inform all other users.
        /// </summary>
        /// <param name="newStatus">New status of user</param>
        public void ChangeStatus(UserStatus newStatus)
        {
            //Get ChatClient object
            var senderClient = _clients[CurrentClient.ClientId];
            if (senderClient == null)
            {
                throw new ApplicationException("Can not change state before login.");
            }

            //Set new status
            senderClient.User.Status = newStatus;

            //Send status of user to all online users
            Task.Factory.StartNew(
                () =>
                {
                    foreach (var chatClient in _clients.GetAllItems())
                    {
                        try
                        {
                            chatClient.ClientProxy.OnUserStatusChange(senderClient.User.Nick, newStatus);
                        }
                        catch
                        {

                        }
                    }
                });
        }

        /// <summary>
        /// Used to logout from chat service.
        /// Client may not call this method while logging out (in an application crash situation),
        /// it will also be logged out automatically when connection fails between client and server.
        /// </summary>
        public void Logout()
        {
            ClientLogout(CurrentClient.ClientId);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles Disconnected event of all clients.
        /// </summary>
        /// <param name="sender">Client object that is disconnected</param>
        /// <param name="e">Event arguments (not used in this event)</param>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            //Get client object
            var client = (IScsServiceClient)sender;

            //Perform logout (so, if client did not call Logout method before close,
            //we do logout automatically.
            ClientLogout(client.ClientId);
        }

        /// <summary>
        /// This method is used to send list of all online users to a new joined user.
        /// </summary>
        /// <param name="client">New user that is joined to service</param>
        private void SendUserListToClient(ChatClient client)
        {
            //Get all users except new user
            var userList = UserList.Where((user) => (user.Nick != client.User.Nick)).ToArray();

            //Do not send list if no user available (except the new user)
            if (userList.Length <= 0)
            {
                return;
            }

            client.ClientProxy.GetUserList(userList);
        }

        /// <summary>
        /// This method is called when a client Calls Logout method of service or a client
        /// connection fails.
        /// </summary>
        /// <param name="clientId">Unique Id of client that is logged out</param>
        private void ClientLogout(long clientId)
        {
            //Get client from client list, if not in list do not continue
            var client = _clients[clientId];
            if (client == null)
            {
                return;
            }

            //Remove client from online clients list
            _clients.Remove(client.Client.ClientId);

            //Unregister to Disconnected event (not needed really)
            client.Client.Disconnected -= Client_Disconnected;

            //Start a new task to inform all other users
            Task.Factory.StartNew(
                () =>
                {
                    OnUserListChanged();
                    SendUserLogoutInfoToAllClients(client.User.Nick);
                });
        }

        /// <summary>
        /// This method is used to inform all online clients
        /// that a new user joined to room.
        /// </summary>
        /// <param name="userInfo">New joined user's informations</param>
        private void SendUserLoginInfoToAllClients(UserInfo userInfo)
        {
            foreach (var client in _clients.GetAllItems())
            {
                //Do not send informations to user that is logged in.
                if (client.User.Nick == userInfo.Nick)
                {
                    continue;
                }

                try
                {
                    client.ClientProxy.OnUserLogin(userInfo);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// This method is used to inform all online clients
        /// that a user disconnected from chat server.
        /// </summary>
        /// <param name="nick">Nick of disconnected user</param>
        private void SendUserLogoutInfoToAllClients(string nick)
        {
            foreach (var client in _clients.GetAllItems())
            {
                try
                {
                    client.ClientProxy.OnUserLogout(nick);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Finds ChatClient ojbect by given nick.
        /// </summary>
        /// <param name="nick">Nick to search</param>
        /// <returns>Found ChatClient for that nick, or null if not found</returns>
        private ChatClient FindClientByNick(string nick)
        {
            return (from client in _clients.GetAllItems()
                    where client.User.Nick == nick
                    select client).FirstOrDefault();
        }

        #endregion

        #region Event raising methods

        /// <summary>
        /// Raises UserListChanged event.
        /// </summary>
        private void OnUserListChanged()
        {
            var handler = UserListChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Sub classes

        /// <summary>
        /// This class is used to store informations for a connected client.
        /// </summary>
        private sealed class ChatClient
        {
            /// <summary>
            /// Scs client reference.
            /// </summary>
            public IScsServiceClient Client { get; private set; }

            /// <summary>
            /// Proxy object to call remote methods of chat client.
            /// </summary>
            public IChatClient ClientProxy { get; private set; }

            /// <summary>
            /// User informations of client.
            /// </summary>
            public UserInfo User { get; private set; }

            /// <summary>
            /// Creates a new ChatClient object.
            /// </summary>
            /// <param name="client">Scs client reference</param>
            /// <param name="clientProxy">Proxy object to call remote methods of chat client</param>
            /// <param name="userInfo">User informations of client</param>
            public ChatClient(IScsServiceClient client, IChatClient clientProxy, UserInfo userInfo)
            {
                Client = client;
                ClientProxy = clientProxy;
                User = userInfo;
            }
        }

        #endregion
    }
}
