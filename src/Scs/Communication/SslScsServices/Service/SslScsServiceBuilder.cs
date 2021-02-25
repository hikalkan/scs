using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Hik.Communication.Scs.Communication.EndPoints;
using Hik.Communication.Scs.Server;
using Hik.Communication.ScsServices.Service;
using Hik.Communication.SslScs.Authentication;
using Hik.Communication.SslScs.Server;

namespace Hik.Communication.SslScsServices.Service
{
    /// <summary>
    /// 
    /// </summary>
    public static class SslScsServiceBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="serverCert"></param>
        /// <param name="clientCerts"></param>
        /// <param name="authMode"></param>
        /// <returns></returns>
        public static IScsServiceApplication CreateSslService(ScsEndPoint endPoint, X509Certificate serverCert, List<X509Certificate2> clientCerts, SslScsAuthMode authMode)
        {
            return new ScsServiceApplication(SslScsServerFactory.CreateSslServer(endPoint, serverCert, clientCerts, authMode));
        }
    }
}
