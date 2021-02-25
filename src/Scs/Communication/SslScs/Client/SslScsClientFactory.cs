using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.SslScs.Authentication;

namespace Hik.Communication.SslScs.Client
{
    /// <summary>
    /// This class is used to create SSL SCS Clients to connect to an SCS server.
    /// </summary>
    public static class SslScsClientFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="serverCertificate"></param>
        /// <param name="sslHostAddress"></param>
        /// <param name="authMode"></param>
        /// <param name="clientCertificate"></param>
        /// <returns></returns>
        public static IScsClient CreateSslClient(ScsEndPoint endpoint, X509Certificate2 serverCertificate
            , string sslHostAddress = ""
            ,SslScsAuthMode authMode=SslScsAuthMode.ServerAuth, X509Certificate clientCertificate=null )
        {
            return endpoint.CreateSslClient(serverCertificate,authMode, clientCertificate, sslHostAddress);
        }
    }
}
