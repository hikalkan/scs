using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.ScsServices.Client;
using Hik.Communication.SslScs.Authentication;

namespace Hik.Communication.SslScsServices.Client
{
    /// <summary>
    /// 
    /// </summary>
    public class SslScsServiceClientBuilder
    {
        /// <summary>
        /// SSL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serverCert"></param>
        /// <param name="clientCert"></param>
        /// <param name="nombreServerCert"></param>
        /// <param name="endpoint"></param>
        /// <param name="clientObject"></param>
        /// <returns></returns>
        public static IScsServiceClient<T> CreateSslClient<T>(ScsEndPoint endpoint,X509Certificate2 serverCert, string hostAddress, SslScsAuthMode authMode, X509Certificate2 clientCert, object clientObject = null) where T : class
        {
            return new ScsServiceClient<T>(endpoint.CreateSslClient(serverCert,authMode,clientCert, hostAddress), clientObject);
        }

    }
}
