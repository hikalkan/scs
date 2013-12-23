using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading;
using Hik.Communication.Scs.Communication.EndPoints.Pipes;

namespace Hik.Communication.Scs.Communication.Channels.Pipes
{
    internal class NamedPipeConnectionListener : ConnectionListenerBase
    {
        #region Constants

        private const int MAX_SERVER_INSTANCES = 16;

        #endregion

        #region Private fields

        private readonly NamedPipeEndPoint _endPoint;
        private readonly object _syncObject;

        private readonly List<NamedPipeCommunicationChannel> _openChannels;
        private NamedPipeServerStream _waitingPipe;

        private bool _running;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeConnectionListener"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public NamedPipeConnectionListener(NamedPipeEndPoint endPoint)
        {
            _syncObject = new object();
            _openChannels = new List<NamedPipeCommunicationChannel>();
            _endPoint = endPoint;
        }

        #region Overrides of ConnectionListenerBase

        /// <inheritdoc />
        public override void Start()
        {
            _running = true;
            OpenPipe();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            _running = false;

            lock (_syncObject)
            {
                if (_waitingPipe != null)
                {
                    try
                    {
                        _waitingPipe.Close();
                        _waitingPipe.Dispose();
                        _waitingPipe = null;
                    }
                    catch { }
                }

                foreach (var channel in _openChannels)
                    channel.Disconnect();
            }
        }

        #endregion

        #region Private Methods

        private void ChannelDisconnected(object sender, EventArgs eventArgs)
        {
            if (!_running) return;
            
            // remove disconnected channel from list
            lock (_syncObject) _openChannels.Remove((NamedPipeCommunicationChannel)sender);
        }

        /// <summary>
        /// Background listening thread.
        /// </summary>
        private void OpenPipe()
        {
            while (_running)
            {
                lock (_syncObject)
                {
                    try
                    {
                        _waitingPipe = new NamedPipeServerStream(_endPoint.Name, PipeDirection.InOut, MAX_SERVER_INSTANCES, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough);

                        _waitingPipe.BeginWaitForConnection(ar =>
                        {
                            try
                            {
                                NamedPipeCommunicationChannel channel;

                                lock (_syncObject)
                                {
                                    _waitingPipe.EndWaitForConnection(ar);
                                    channel = new NamedPipeCommunicationChannel(_endPoint, _waitingPipe);
                                    _waitingPipe = null;
                                }

                                channel.Disconnected += ChannelDisconnected;
                                OnCommunicationChannelConnected(channel);
                            }
                            catch (ObjectDisposedException)
                            { }

                            // listen for next connection
                            if (_running) OpenPipe();
                        }, null);

                        return;
                    }
                    catch
                    {
                        if (_waitingPipe != null)
                        {
                            try
                            {
                                _waitingPipe.Dispose();
                            }
                            catch { }
                            _waitingPipe = null;
                        }
                    }
                }

                if (_running) Thread.Sleep(1000);
            }
        }

        #endregion
    }
}
