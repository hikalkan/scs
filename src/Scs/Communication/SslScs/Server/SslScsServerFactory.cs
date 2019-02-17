using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Server;
using Hik.Communication.SslScs.Authentication;

namespace Hik.Communication.SslScs.Server
{
    public static class SslScsServerFactory
    {
        /// <summary>
        /// SSL
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static IScsServer CreateSslServer(ScsEndPoint endPoint, X509Certificate serverCert, List<X509Certificate2> clientCerts, SslScsAuthMode authMode)
        {
            return endPoint.CreateSslServer(serverCert, clientCerts, authMode);
        }
    }
}
