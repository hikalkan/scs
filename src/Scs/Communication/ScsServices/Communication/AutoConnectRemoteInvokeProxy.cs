using System.Runtime.Remoting.Messaging;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication;
using Hik.Communication.Scs.Communication.Messengers;

namespace Hik.Communication.ScsServices.Communication
{
    /// <summary>
    /// This class extends RemoteInvokeProxy to provide auto connect/disconnect mechanism
    /// if client is not connected to the server when a service method is called.
    /// </summary>
    /// <typeparam name="TProxy">Type of the proxy class/interface</typeparam>
    /// <typeparam name="TMessenger">Type of the messenger object that is used to send/receive messages</typeparam>
    internal class AutoConnectRemoteInvokeProxy<TProxy, TMessenger> : RemoteInvokeProxy<TProxy, TMessenger> where TMessenger : IMessenger
    {
        /// <summary>
        /// Reference to the client object that is used to connect/disconnect.
        /// </summary>
        private readonly IConnectableClient _client;

        /// <summary>
        /// Creates a new AutoConnectRemoteInvokeProxy object.
        /// </summary>
        /// <param name="clientMessenger">Messenger object that is used to send/receive messages</param>
        /// <param name="client">Reference to the client object that is used to connect/disconnect</param>
        public AutoConnectRemoteInvokeProxy(RequestReplyMessenger<TMessenger> clientMessenger, IConnectableClient client)
            : base(clientMessenger)
        {
            _client = client;
        }

        /// <summary>
        /// Overrides message calls and translates them to messages to remote application.
        /// </summary>
        /// <param name="msg">Method invoke message (from RealProxy base class)</param>
        /// <returns>Method invoke return message (to RealProxy base class)</returns>
        public override IMessage Invoke(IMessage msg)
        {
            if (_client.CommunicationState == CommunicationStates.Connected)
            {
                //If already connected, behave as base class (RemoteInvokeProxy).
                return base.Invoke(msg);
            }

            //Connect, call method and finally disconnect
            _client.Connect();
            try
            {
                return base.Invoke(msg);
            }
            finally
            {
                _client.Disconnect();
            }
        }
    }
}
