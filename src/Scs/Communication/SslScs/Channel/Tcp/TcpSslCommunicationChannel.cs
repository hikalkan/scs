using System;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;

namespace Hik.Communication.SslScs.Channel.Tcp
{
    /// <summary>
    /// This class is used to communicate with a remote application over TCP/IP protocol.
    /// </summary>
    internal class TcpSslCommunicationChannel : CommunicationChannelBase
    {
        #region Public properties

        ///<summary>
        /// Gets the endpoint of remote application.
        ///</summary>
        public override ScsEndPoint RemoteEndPoint => _remoteEndPoint;

        private readonly ScsTcpEndPoint _remoteEndPoint;

        #endregion

        #region Private fields

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int ReceiveBufferSize = 4 * 1024; //4KB

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// The socket
        /// </summary>
        private readonly TcpClient _client;

        /// <summary>
        /// The SSL stream
        /// </summary>
        private readonly SslStream _sslStream;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new TcpCommunicationChannel object.
        /// </summary>
        public TcpSslCommunicationChannel(TcpClient client, SslStream sslStream)
        {
            _client = client;
            _client.NoDelay = true;
            _sslStream = sslStream;
            var endpoint = (IPEndPoint) client.Client.RemoteEndPoint; 

            _remoteEndPoint = new ScsTcpEndPoint(endpoint.Address.ToString(), endpoint.Port);
            _buffer = new byte[ReceiveBufferSize];
            _syncLock = new object();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Disconnects from remote application and closes channel.
        /// </summary>
        public override void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            _running = false;
            try
            {
                if (_client.Connected)
                {
                    _client.Close();
                }
            }
            catch (Exception exception)
            {
                Trace.Write($"Disconnect: {exception}");
            }

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Starts the thread to receive messages from socket.
        /// </summary>
        protected override void StartInternal()
        {
            _running = true;
            _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);
        }

        /// <summary>
        /// Sends a message to the remote application.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        protected override void SendMessageInternal(IScsMessage message)
        {
            //Send message
            var totalSent = 0;
            lock (_syncLock)
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);
                //Send all bytes to the remote application

                try
                {
                    _sslStream.Write(messageBytes, totalSent, messageBytes.Length);
                }
                catch
                {
                    throw new CommunicationException("Cannot send data on the SSL stream");
                }


                LastSentMessageTime = DateTime.Now;
                OnMessageSent(message);
            }
        }

        #endregion

        #region Private methods


        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socket.
        /// </summary>
        /// <param name="ar">Asyncronous call result</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!_running)
            {
                return;
            }

            try
            {
                //Get received bytes count
                var bytesRead = _sslStream.EndRead(ar);

                if (bytesRead > 0)
                {              
                    LastReceivedMessageTime = DateTime.Now;
                    //Copy received bytes to a new byte array
                    var receivedBytes = new byte[bytesRead];
                    Array.Copy(_buffer, 0, receivedBytes,  0, bytesRead);
                    try
                    {
                        //Read messages according to current wire protocol
                        var messages = WireProtocol.CreateMessages(receivedBytes);
                        //Raise MessageReceived event for all received messages
                        foreach (var message in messages)
                        {
                            OnMessageReceived(message);
                        }
                    }
                    catch (SerializationException ex)
                    {
                        Trace.Write($"Error while deserializing message: {ex}");
                    }
                }
                else
                {
                    throw new CommunicationException("Tcp socket is closed");
                }

                //Read more bytes if still running
                if (_running)
                {
                    _sslStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);
                }
            }
            catch (Exception exception)
            {
                Trace.Write($"ReceiveCallback: {exception}");
                Disconnect();
            }
        }
        #endregion
    }
}
