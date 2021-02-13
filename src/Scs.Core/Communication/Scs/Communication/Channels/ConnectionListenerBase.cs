using System;

namespace Hik.Communication.Scs.Communication.Channels
{
    /// <summary>
    /// This class provides base functionality for communication listener classes.
    /// </summary>
    internal abstract class ConnectionListenerBase : IConnectionListener
    {
        /// <summary>
        /// This event is raised when a new communication channel is connected.
        /// </summary>
        public event EventHandler<CommunicationChannelEventArgs> CommunicationChannelConnected;

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        public abstract void Start();
        
        /// <summary>
        /// Stops listening incoming connections.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Raises CommunicationChannelConnected event.
        /// </summary>
        /// <param name="client"></param>
        protected virtual void OnCommunicationChannelConnected(ICommunicationChannel client)
        {
            var handler = CommunicationChannelConnected;
            if (handler != null)
            {
                handler(this, new CommunicationChannelEventArgs(client));
            }
        }
    }
}
