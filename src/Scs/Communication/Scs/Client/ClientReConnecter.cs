using System;
using Hik.Communication.Scs.Communication;
using Hik.Threading;

namespace Hik.Communication.Scs.Client
{
    /// <summary>
    /// This class is used to automatically re-connect to server if disconnected.
    /// It attempts to reconnect to server periodically until connection established.
    /// </summary>
    public class ClientReConnecter : IDisposable
    {
        /// <summary>
        /// Reconnect check period.
        /// Default: 20 seconds.
        /// </summary>
        public int ReConnectCheckPeriod
        {
            get { return _reconnectTimer.Period; }
            set { _reconnectTimer.Period = value; }
        }

        /// <summary>
        /// Reference to client object.
        /// </summary>
        private readonly IConnectableClient _client;

        /// <summary>
        /// Timer to  attempt ro reconnect periodically.
        /// </summary>
        private readonly Timer _reconnectTimer;

        /// <summary>
        /// Indicates the dispose state of this object.
        /// </summary>
        private volatile bool _disposed;

        /// <summary>
        /// Creates a new ClientReConnecter object.
        /// It is not needed to start ClientReConnecter since it automatically
        /// starts when the client disconnected.
        /// </summary>
        /// <param name="client">Reference to client object</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if client is null.</exception>
        public ClientReConnecter(IConnectableClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            _client = client;
            _client.Disconnected += Client_Disconnected;
            _reconnectTimer = new Timer(20000);
            _reconnectTimer.Elapsed += ReconnectTimer_Elapsed;
            _reconnectTimer.Start();
        }

        /// <summary>
        /// Disposes this object.
        /// Does nothing if already disposed.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _client.Disconnected -= Client_Disconnected;
            _reconnectTimer.Stop();
        }

        /// <summary>
        /// Handles Disconnected event of _client object.
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event arguments</param>
        private void Client_Disconnected(object sender, EventArgs e)
        {
            _reconnectTimer.Start();
        }

        /// <summary>
        /// Hadles Elapsed event of _reconnectTimer.
        /// </summary>
        /// <param name="sender">Source of the event</param>
        /// <param name="e">Event arguments</param>
        private void ReconnectTimer_Elapsed(object sender, EventArgs e)
        {
            if (_disposed || _client.CommunicationState == CommunicationStates.Connected)
            {
                _reconnectTimer.Stop();
                return;
            }

            try
            {
                _client.Connect();
                _reconnectTimer.Stop();
            }
            catch
            {
                //No need to catch since it will try to re-connect again
            }
        }
    }
}
