using System;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Communication.Messengers;

namespace Hik.Communication.Scs.Server
{
    /// <summary>
    /// Represents a client from a perspective of a server.
    /// </summary>
    public interface IScsServerClient : IMessenger
    {
        /// <summary>
        /// This event is raised when client disconnected from server.
        /// </summary>
        event EventHandler Disconnected;
        
        /// <summary>
        /// Unique identifier for this client in server.
        /// </summary>
        long ClientId { get; }

        ///<summary>
        /// Gets endpoint of remote application.
        ///</summary>
        ScsEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Gets the current communication state.
        /// </summary>
        CommunicationStates CommunicationState { get; }

        /// <summary>
        /// Disconnects from server.
        /// </summary>
        void Disconnect();
    }
}
