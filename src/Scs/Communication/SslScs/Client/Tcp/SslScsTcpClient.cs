using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.SslScs.Authentication;
using Hik.Communication.SslScs.Channel.Tcp;


namespace Hik.Communication.SslScs.Client.Tcp
{
   
    /// <inheritdoc />
    /// <summary>
    /// This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    internal class SslScsTcpClient : ScsClientBase
    {
        /// <summary>
        /// The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        private readonly X509Certificate2 _serverCertificate;
        private readonly X509Certificate _clientCertificate;
        private readonly SslScsAuthMode _sslAuthenticationMechanism;
        private readonly string _sslHostAddress;
        private readonly RemoteCertificateValidationCallback _validateCertificate;


        /// <summary>
        /// Creates a new ScsTcpClient object.
        /// </summary>
        /// <param name="serverEndPoint">The endpoint address to connect to the server</param>
        /// <param name="serverCertificate">The server certificate (public key only)</param>
        /// <param name="sslHostAddress">SSL Host address that will be used for authentication</param>
        /// <param name="authenticationMechanism">The used authentication mechanism</param>
        public SslScsTcpClient(ScsTcpEndPoint serverEndPoint, X509Certificate2 serverCertificate
            , string sslHostAddress
            , SslScsAuthMode authenticationMechanism = SslScsAuthMode.ServerAuth
            )
        {
            _serverEndPoint = serverEndPoint;
            _serverCertificate = serverCertificate;
            _sslHostAddress = sslHostAddress;
            _sslAuthenticationMechanism = authenticationMechanism;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverEndPoint">The endpoint address to connect to the server</param>
        /// <param name="serverCertificate">The server certificate (public key only)</param>
        /// <param name="sslHostAddress">SSL Host address that will be used for authentication</param>
        /// <param name="authenticationMechanism">The used authentication mechanism</param>
        /// <param name="clientCertificate">The client certificate (public and private keys)
        /// This is optional and it is used in case of mutual authentication</param>
        public SslScsTcpClient(ScsTcpEndPoint serverEndPoint, X509Certificate2 serverCertificate
            , string sslHostAddress
            , SslScsAuthMode authenticationMechanism
            , X509Certificate clientCertificate)
            : this(serverEndPoint, serverCertificate, sslHostAddress, authenticationMechanism)
        {
            _clientCertificate = clientCertificate;
        }

        public SslScsTcpClient(ScsTcpEndPoint serverEndPoint, X509Certificate2 serverCertificate
            , string sslHostAddress
            , SslScsAuthMode authenticationMechanism
            , X509Certificate clientCertificate
            , RemoteCertificateValidationCallback validateCertificate)
            : this(serverEndPoint, serverCertificate, sslHostAddress, authenticationMechanism, clientCertificate)
        {
            _validateCertificate = validateCertificate;
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            var client = new TcpClient();
           SslStream sslStream=null;
            try
            {
                client = new TcpClient();
                client.Connect(new IPEndPoint(IPAddress.Parse(_serverEndPoint.IpAddress), _serverEndPoint.TcpPort));

                 sslStream = new SslStream(client.GetStream(), false,
                    _validateCertificate ?? DefaultValidateCertificate,
                    SelectLocalCertificate);

                switch (_sslAuthenticationMechanism)
                {
                    case SslScsAuthMode.ServerAuth:
                        sslStream.AuthenticateAsClient(_sslHostAddress, null, SslProtocols.Default, false);
                        break;
                    case SslScsAuthMode.MutualAuth:
                        if (_clientCertificate == null) throw new ArgumentNullException("Client certificate cannot be null for mutual authentication");
                        var clientCertificates = new X509Certificate2Collection { _clientCertificate };
                        sslStream.AuthenticateAsClient(_sslHostAddress, clientCertificates, SslProtocols.Default, false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


                return new TcpSslCommunicationChannel( client, sslStream);
            }
            catch (AuthenticationException )
            {
                sslStream?.Close();

                client.Close();
                throw;
            }


        }


        public bool DefaultValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            switch (_sslAuthenticationMechanism)
            {
                case SslScsAuthMode.ServerAuth:
                    return _serverCertificate.GetPublicKeyString() == certificate.GetPublicKeyString();
                  
                case SslScsAuthMode.MutualAuth:
                    return _serverCertificate.GetCertHashString().Equals(certificate.GetCertHashString());
                    //if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                    //{
                    //    return _serverCertificate.GetCertHashString().Equals(certificate.GetCertHashString());
                    //}
                    //else
                    //{
                    //    return (sslPolicyErrors == SslPolicyErrors.None);
                    //}
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public X509Certificate SelectLocalCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return _clientCertificate;
        }

    }
}
