using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.SslScs.Authentication;

namespace Hik.Communication.SslScs.Channel.Tcp
{
    /// <summary>
    /// This class is used to listen and accept incoming TCP
    /// connection requests on a TCP port.
    /// </summary>
    internal class TcpSslConnectionListener : ConnectionListenerBase
    {
        /// <summary>
        /// The endpoint address of the server to listen incoming connections.
        /// </summary>
        private readonly ScsTcpEndPoint _endPoint;

        /// <summary>
        /// Server socket to listen incoming connection requests.
        /// </summary>
        private TcpListener _listenerSocket;

        /// <summary>
        /// The thread to listen socket
        /// </summary>
        private Thread _thread;

        /// <summary>
        /// A flag to control thread's running
        /// </summary>
        private volatile bool _running;

        private readonly X509Certificate _serverCertificate;
        private readonly List<X509Certificate2> _clientCertificates;

        private readonly SslScsAuthMode _authMode;
        private readonly RemoteCertificateValidationCallback _validationCallback;

        /// <summary>
        /// Creates a new TcpConnectionListener for given endpoint.
        /// </summary>
        /// <param name="endPoint">The endpoint address of the server to listen incoming connections</param>
        /// <param name="serverCert"></param>
        /// <param name="clientCerts"></param>
        /// <param name="authMode"></param>
        /// <param name="validationCallback"></param>
        public TcpSslConnectionListener(ScsTcpEndPoint endPoint, X509Certificate serverCert
            , List<X509Certificate2> clientCerts
            , SslScsAuthMode authMode
            , RemoteCertificateValidationCallback validationCallback = null)
        {
            _endPoint = endPoint;
            _serverCertificate = serverCert;
            _clientCertificates = clientCerts;
            _authMode = authMode;
            _validationCallback = validationCallback;
        }

        /// <summary>
        /// Starts listening incoming connections.
        /// </summary>
        public override void Start()
        {
            StartSocket();
            _running = true;
            _thread = new Thread(DoListenAsThread);
            _thread.Start();
        }

        /// <summary>
        /// Stops listening incoming connections.
        /// </summary>
        public override void Stop()
        {
            _running = false;
            StopSocket();
        }

        /// <summary>
        /// Starts listening socket.
        /// </summary>
        private void StartSocket()
        {
            _listenerSocket = new TcpListener(System.Net.IPAddress.Any, _endPoint.TcpPort);
            _listenerSocket.Start();
        }

        /// <summary>
        /// Stops listening socket.
        /// </summary>
        private void StopSocket()
        {
            try
            {
                _listenerSocket.Stop();
            }
            catch (Exception exception)
            {
                Trace.Write($"StopSocket: {exception}");
            }
        }

        /// <summary>
        /// Entrance point of the thread.
        /// This method is used by the thread to listen incoming requests.
        /// </summary>
        private void DoListenAsThread()
        {
            while (_running)
            {
                try
                {
                    var client = _listenerSocket.AcceptTcpClient();
                    if (client.Connected)
                    {
                        var sslStream = new SslStream(client.GetStream(), false, _validationCallback ?? ValidateCertificate);
                        switch (_authMode)
                        {
                            case SslScsAuthMode.ServerAuth:
                                sslStream.AuthenticateAsServer(_serverCertificate, false, System.Security.Authentication.SslProtocols.Default, false);
                                break;
                            case SslScsAuthMode.MutualAuth:
                                sslStream.AuthenticateAsServer(_serverCertificate, true, System.Security.Authentication.SslProtocols.Default, false);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        OnCommunicationChannelConnected(new TcpSslCommunicationChannel(_endPoint, client, sslStream));
                    }
                }
                catch
                {
                    //Disconnect, wait for a while and connect again.
                    StopSocket();
                    Thread.Sleep(1000);
                    if (!_running)
                    {
                        return;
                    }

                    try
                    {
                        StartSocket();
                    }
                    catch (Exception exception)
                    {
                        Trace.Write($"DoListenAsThread: {exception}");
                    }
                }
            }
        }
        /// <summary>
        /// Default Validation mechanism
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            switch (_authMode)
            {
                case SslScsAuthMode.ServerAuth:
                    return true;//Here the client will not send a certificate

                case SslScsAuthMode.MutualAuth:
                    return MutualAuthentication(certificate, sslPolicyErrors);
        
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        /// <summary>
        /// Default mutual authentication validation mechanism
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool MutualAuthentication(X509Certificate certificate, SslPolicyErrors sslPolicyErrors)
        {
            if (_clientCertificates == null)
            {
                return false;
            }

            if ((sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors) && (_clientCertificates != null))
            {
                foreach (var clientCert in _clientCertificates)
                {

                    if (clientCert.GetCertHashString().Equals(certificate.GetCertHashString()))
                        return true;
                }

                return false;
            }
            else
            {
                return (sslPolicyErrors == SslPolicyErrors.None);
            }
        }
    }
}