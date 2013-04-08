using System;
using Hik.Collections;
using Hik.Communication.Scs.Communication.Protocols;

namespace Hik.Communication.Scs.Server
{
    /// <summary>
    /// Represents a SCS server that is used to accept and manage client connections.
    /// </summary>
    public interface IScsServer
    {
        /// <summary>
        /// This event is raised when a new client connected to the server.
        /// </summary>
        event EventHandler<ServerClientEventArgs> ClientConnected;

        /// <summary>
        /// This event is raised when a client disconnected from the server.
        /// </summary>
        event EventHandler<ServerClientEventArgs> ClientDisconnected;

        /// <summary>
        /// Gets/sets wire protocol factory to create IWireProtocol objects.
        /// </summary>
        IScsWireProtocolFactory WireProtocolFactory { get; set; }

        /// <summary>
        /// A collection of clients that are connected to the server.
        /// </summary>
        ThreadSafeSortedList<long, IScsServerClient> Clients { get; }
        
        /// <summary>
        /// Starts the server.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the server.
        /// </summary>
        void Stop();
    }
}
