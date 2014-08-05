using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Messengers;

namespace Hik.Communication.Scs.Client
{
    /// <summary>
    /// Represents a client to connect to server.
    /// </summary>
    public interface IScsClient : IMessenger, IConnectableClient
    {
        /// <summary>
        /// Gets the communication channel for this client.
        /// </summary>
        /// <value>
        /// The communication channel.
        /// </value>
        ICommunicationChannel CommunicationChannel { get; }
    }
}
