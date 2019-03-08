using System;
using Hik.Collections;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Protocols;

namespace Hik.Communication.Scs.Server
{
    /// <summary>
    /// This class provides base functionality for server classes.
    /// </summary>
    internal abstract class ScsServerBase : IScsServer
    {
        #region Public events

        /// <summary>
        /// This event is raised when a new client is connected.
        /// </summary>
        public event EventHandler<ServerClientEventArgs> ClientConnected;

        /// <summary>
        /// This event is raised when a client disconnected from the server.
        /// </summary>
        public event EventHandler<ServerClientEventArgs> ClientDisconnected;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        public IScsWireProtocolFactory WireProtocolFactory { get; set; }

        /// <summary>
        /// A collection of clients that are connected to the server.
        /// </summary>
        public ThreadSafeSortedList<long, IScsServerClient> Clients { get; private set; }

        #endregion

        #region Private properties

        /// <summary>
        /// This object is used to listen incoming connections.
        /// </summary>
        private IConnectionListener _connectionListener;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ScsServerBase()
        {
            Clients = new ThreadSafeSortedList<long, IScsServerClient>();
            WireProtocolFactory = WireProtocolManager.GetDefaultWireProtocolFactory();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts the server.
        /// </summary>
        public virtual void Start()
        {
            _connectionListener = CreateConnectionListener();
            _connectionListener.CommunicationChannelConnected += ConnectionListener_CommunicationChannelConnected;
            _connectionListener.Start();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        public virtual void Stop()
        {
            if (_connectionListener != null)
            {
                _connectionListener.Stop();
            }

            foreach (var client in Clients.GetAllItems())
            {
                client.Disconnect();
            }
        }

        #endregion

        #region Protected abstract methods

        /// <summary>
        /// This method is implemented by derived classes to create appropriate connection listener to listen incoming connection requets.
        /// </summary>
        /// <returns></returns>
        protected abstract IConnectionListener CreateConnectionListener();

        #endregion

        #region Private methods

        /// <summary>
        /// Handles CommunicationChannelConnected event of _connectionListener object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void ConnectionListener_CommunicationChannelConnected(object sender, CommunicationChannelEventArgs e)
        {
            var client = new ScsServerClient(e.Channel)
            {
                ClientId = ScsServerManager.GetClientId(),
                WireProtocol = WireProtocolFactory.CreateWireProtocol()
            };

            client.Disconnected += Client_Disconnected;
            Clients[client.ClientId] = client;
            OnClientConnected(client);
            e.Channel.Start();
        }

        /// <summary>
        /// Handles Disconnected events of all connected clients.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            var client = (IScsServerClient) sender;
            Clients.Remove(client.ClientId);
            OnClientDisconnected(client);
        }

        #endregion

        #region Event raising methods

        /// <summary>
        /// Raises ClientConnected event.
        /// </summary>
        /// <param name="client">Connected client</param>
        protected virtual void OnClientConnected(IScsServerClient client)
        {
            var handler = ClientConnected;
            if (handler != null)
            {
                handler(this, new ServerClientEventArgs(client));
            }
        }

        /// <summary>
        /// Raises ClientDisconnected event.
        /// </summary>
        /// <param name="client">Disconnected client</param>
        protected virtual void OnClientDisconnected(IScsServerClient client)
        {
            var handler = ClientDisconnected;
            if (handler != null)
            {
                handler(this, new ServerClientEventArgs(client));
            }
        }

        #endregion
    }
}
