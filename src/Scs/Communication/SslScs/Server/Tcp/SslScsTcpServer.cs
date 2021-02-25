using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Server;
using Hik.Communication.SslScs.Authentication;
using Hik.Communication.SslScs.Channel.Tcp;

namespace Hik.Communication.SslScs.Server.Tcp
{
    /// <summary>
    /// This class is used to create a TCP server.
    /// </summary>
    internal class ScsSslTcpServer : ScsServerBase
    {
        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        private readonly X509Certificate _serverCert;
        private readonly List<X509Certificate2> _clientCerts;
        private readonly SslScsAuthMode _authMode;

        /// <summary>
        /// Creates a new ScsTcpServer object.
        /// </summary>
        /// <param name="endPoint">The endpoint address of the server to listen incoming connections</param>
        /// <param name="serverCert"></param>
        /// <param name="clientCerts"></param>
        /// <param name="authMode"></param>
        public ScsSslTcpServer(ScsTcpEndPoint endPoint, X509Certificate serverCert, List<X509Certificate2> clientCerts, SslScsAuthMode authMode)
        {
            _endPoint = endPoint;
            _serverCert = serverCert;
            _clientCerts = clientCerts;
            _authMode = authMode;
        }

        /// <summary>
        /// Creates a TCP connection listener.
        /// </summary>
        /// <returns>Created listener object</returns>
        protected override IConnectionListener CreateConnectionListener()
        {
            return new TcpSslConnectionListener(_endPoint, _serverCert, _clientCerts, _authMode);
        }
    }
}
