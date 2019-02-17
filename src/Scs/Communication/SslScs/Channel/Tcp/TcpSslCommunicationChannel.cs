using System;
using System.Diagnostics;
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
        public TcpSslCommunicationChannel(ScsTcpEndPoint endPoint, TcpClient client, SslStream sslStream)
        {
            _client = client;
            _client.NoDelay = true;
            _sslStream = sslStream;
            _remoteEndPoint = endPoint;
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
        /// This buffer is used to resolve an issue with EndRead function for _sslStream
        /// </summary>
        private byte _secondBuffer;
        private bool _secondBufferFlag;
        /// <summary>
        /// This method is used as callback method in _clientSocket's BeginReceive method.
        /// It reveives bytes from socker.
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
                //TODO:: There is an issue with the EndRead, where it reads only 1 byte of the first message then reads the rest of the message
                //this should be fixed. 
                //Get received bytes count
                var bytesRead = _sslStream.EndRead(ar);

                if (bytesRead > 0)
                {
                    if (bytesRead == 1)
                    {
                        _secondBuffer = _buffer[0];
                        _secondBufferFlag = true;
                    }
                    else
                    {
                        LastReceivedMessageTime = DateTime.Now;
                        //Copy received bytes to a new byte array
                        var receivedBytes = new byte[(_secondBufferFlag) ? bytesRead + 1 : bytesRead];
                        if (_secondBufferFlag) receivedBytes[0] = _secondBuffer;
                        Array.Copy(_buffer, 0, receivedBytes, (_secondBufferFlag) ? 1 : 0, bytesRead);
                        _secondBufferFlag = false;
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
