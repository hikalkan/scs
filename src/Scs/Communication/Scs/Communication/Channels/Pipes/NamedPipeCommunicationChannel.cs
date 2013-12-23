using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Communication.EndPoints.Pipes;
using Hik.Communication.Scs.Communication.Messages;

namespace Hik.Communication.Scs.Communication.Channels.Pipes
{
    /// <summary>
    /// Named pipe implementation of <see cref="ICommunicationChannel"/>.
    /// </summary>
    internal class NamedPipeCommunicationChannel : CommunicationChannelBase
    {
        #region Constants

        private const int CONNECT_ATTEMPT_INTERVAL_MS = 100;

        #endregion

        #region Private fields

        /// <summary>
        /// The end point.
        /// </summary>
        private readonly NamedPipeEndPoint _endPoint;

        /// <summary>
        /// Size of the buffer that is used to receive bytes from TCP socket.
        /// </summary>
        private const int ReceiveBufferSize = 4 * 1024; //4KB

        /// <summary>
        /// This buffer is used to receive bytes 
        /// </summary>
        private readonly byte[] _buffer;

        /// <summary>
        /// Socket object to send/receive messages.
        /// </summary>
        private readonly PipeStream _pipeStream;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        /// <summary>
        /// This object is just used for thread synchronizing (locking).
        /// </summary>
        private readonly object _syncLock;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeCommunicationChannel" /> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="pipeStream">An existing pipe stream or <c>null</c> to create client.</param>
        public NamedPipeCommunicationChannel(NamedPipeEndPoint endPoint,  PipeStream pipeStream = null)
        {
            _endPoint = endPoint;

            if (pipeStream != null) _pipeStream = pipeStream;
            else
            {
                var client = new NamedPipeClientStream(".", endPoint.Name, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

                // connect overload with timeout is extremely processor intensive
                int elapsed = 0, connectionTimeout = endPoint.ConnectionTimeout * 1000;

                CONNECT:
                try
                {
                    client.Connect(0);
                }
                catch (TimeoutException)
                {
                    Thread.Sleep(CONNECT_ATTEMPT_INTERVAL_MS);

                    if (endPoint.ConnectionTimeout != Timeout.Infinite && (elapsed += CONNECT_ATTEMPT_INTERVAL_MS) > connectionTimeout)
                        throw new TimeoutException("The host failed to connect. Timeout occurred.");

                    goto CONNECT;
                }

                _pipeStream = client;
            }

            _buffer = new byte[ReceiveBufferSize];
            _syncLock = new object();
        }

        #region Overrides of CommunicationChannelBase

        /// <inheritdoc />
        public override ScsEndPoint RemoteEndPoint
        {
            get { return _endPoint; }
        }

        /// <inheritdoc />
        public override void Disconnect()
        {
            if (CommunicationState != CommunicationStates.Connected)
            {
                return;
            }

            _running = false;
            try
            {
                _pipeStream.Close();
                _pipeStream.Dispose();
            }
            catch { }

            CommunicationState = CommunicationStates.Disconnected;
            OnDisconnected();
        }

        /// <inheritdoc />
        protected override void StartInternal()
        {
            _running = true;
            _pipeStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);
        }

        /// <inheritdoc />
        protected override void SendMessageInternal(IScsMessage message)
        {
            //Send message
            lock (_syncLock)
            {
                //Create a byte array from message according to current protocol
                var messageBytes = WireProtocol.GetBytes(message);

                try
                {
                    // send bytes
                    _pipeStream.Write(messageBytes, 0, messageBytes.Length);
                }
                catch (IOException exception)
                {
                    throw new CommunicationException("Failed to send message.", exception);
                }

                LastSentMessageTime = DateTime.Now;
                OnMessageSent(message);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Receive callback for <see cref="_pipeStream"/>.
        /// </summary>
        /// <param name="ar">The async result.</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!_running)
            {
                return;
            }

            try
            {
                //Get received bytes count
                var bytesRead = _pipeStream.EndRead(ar);
                if (bytesRead > 0)
                {
                    LastReceivedMessageTime = DateTime.Now;

                    //Copy received bytes to a new byte array
                    var receivedBytes = new byte[bytesRead];
                    Array.Copy(_buffer, 0, receivedBytes, 0, bytesRead);

                    //Read messages according to current wire protocol
                    var messages = WireProtocol.CreateMessages(receivedBytes);

                    //Raise MessageReceived event for all received messages
                    foreach (var message in messages)
                    {
                        OnMessageReceived(message);
                    }
                }
                else
                {
                    throw new CommunicationException("Named pipe is closed.");
                }

                //Read more bytes if still running
                if (_running)
                {
                    _pipeStream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, null);
                }
            }
            catch
            {
                Disconnect();
            }
        }

        #endregion
    }
}
