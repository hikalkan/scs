using System;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Protocols;
using Hik.Threading;

namespace Hik.Communication.Scs.Client
{
    /// <summary>
    /// This class provides base functionality for client classes.
    /// </summary>
    internal abstract class ScsClientBase : IScsClient
    {
        #region Public events

        /// <summary>
        /// This event is raised when a new message is received.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event is raised when a new message is sent without any error.
        /// It does not guaranties that message is properly handled and processed by remote application.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSent;

        /// <summary>
        /// This event is raised when communication channel closed.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        public event EventHandler Disconnected;

        #endregion

        #region Public properties

        /// <summary>
        /// Timeout for connecting to a server (as milliseconds).
        /// Default value: 15 seconds (15000 ms).
        /// </summary>
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        public IScsWireProtocol WireProtocol
        {
            get { return _wireProtocol; }
            set
            {
                if (CommunicationState == CommunicationStates.Connected)
                {
                    throw new ApplicationException("Wire protocol can not be changed while connected to server.");
                }

                _wireProtocol = value;
            }
        }
        private IScsWireProtocol _wireProtocol;

        /// <summary>
        /// Gets the communication state of the Client.
        /// </summary>
        public CommunicationStates CommunicationState
        {
            get
            {
                return _communicationChannel != null
                           ? _communicationChannel.CommunicationState
                           : CommunicationStates.Disconnected;
            }
        }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastReceivedMessageTime
        {
            get
            {
                return _communicationChannel != null
                           ? _communicationChannel.LastReceivedMessageTime
                           : DateTime.MinValue;
            }
        }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastSentMessageTime
        {
            get
            {
                return _communicationChannel != null
                           ? _communicationChannel.LastSentMessageTime
                           : DateTime.MinValue;
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Default timeout value for connecting a server.
        /// </summary>
        private const int DefaultConnectionAttemptTimeout = 15000; //15 seconds.

        /// <summary>
        /// The communication channel that is used by client to send and receive messages.
        /// </summary>
        private ICommunicationChannel _communicationChannel;

        /// <summary>
        /// This timer is used to send PingMessage messages to server periodically.
        /// </summary>
        private readonly Timer _pingTimer;

        #endregion
        
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ScsClientBase()
        {
            _pingTimer = new Timer(30000);
            _pingTimer.Elapsed += PingTimer_Elapsed;
            ConnectTimeout = DefaultConnectionAttemptTimeout;
            WireProtocol = WireProtocolManager.GetDefaultWireProtocol();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Connects to server.
        /// </summary>
        public void Connect()
        {
            WireProtocol.Reset();
            _communicationChannel = CreateCommunicationChannel();
            _communicationChannel.WireProtocol = WireProtocol;
            _communicationChannel.Disconnected += CommunicationChannel_Disconnected;
            _communicationChannel.MessageReceived += CommunicationChannel_MessageReceived;
            _communicationChannel.MessageSent += CommunicationChannel_MessageSent;
            _communicationChannel.Start();
            _pingTimer.Start();
            OnConnected();
        }

        /// <summary>
        /// Disconnects from server.
        /// Does nothing if already disconnected.
        /// </summary>
        public void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            _communicationChannel.Disconnect();
        }

        /// <summary>
        /// Disposes this object and closes underlying connection.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        /// <exception cref="CommunicationStateException">Throws a CommunicationStateException if client is not connected to the server.</exception>
        public void SendMessage(IScsMessage message)
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                throw new CommunicationStateException("Client is not connected to the server.");
            }

            _communicationChannel.SendMessage(message);
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// This method is implemented by derived classes to create appropriate communication channel.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected abstract ICommunicationChannel CreateCommunicationChannel();

        #endregion

        #region Private methods

        /// <summary>
        /// Handles MessageReceived event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageReceived(object sender, MessageEventArgs e)
        {
            if (e.Message is ScsPingMessage)
            {
                return;
            }

            OnMessageReceived(e.Message);
        }

        /// <summary>
        /// Handles MessageSent event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_MessageSent(object sender, MessageEventArgs e)
        {
            OnMessageSent(e.Message);
        }

        /// <summary>
        /// Handles Disconnected event of _communicationChannel object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void CommunicationChannel_Disconnected(object sender, EventArgs e)
        {
            _pingTimer.Stop();
            OnDisconnected();
        }

        /// <summary>
        /// Handles Elapsed event of _pingTimer to send PingMessage messages to server.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void PingTimer_Elapsed(object sender, EventArgs e)
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            try
            {
                var lastMinute = DateTime.Now.AddMinutes(-1);
                if (_communicationChannel.LastReceivedMessageTime > lastMinute || _communicationChannel.LastSentMessageTime > lastMinute)
                {
                    return;
                }

                _communicationChannel.SendMessage(new ScsPingMessage());
            }
            catch
            {

            }
        }

        #endregion

        #region Event raising methods

        /// <summary>
        /// Raises Connected event.
        /// </summary>
        protected virtual void OnConnected()
        {
            var handler = Connected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises Disconnected event.
        /// </summary>
        protected virtual void OnDisconnected()
        {
            var handler = Disconnected;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises MessageReceived event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageReceived(IScsMessage message)
        {
            var handler = MessageReceived;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(message));
            }
        }

        /// <summary>
        /// Raises MessageSent event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageSent(IScsMessage message)
        {
            var handler = MessageSent;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(message));
            }
        }

        #endregion
    }
}
