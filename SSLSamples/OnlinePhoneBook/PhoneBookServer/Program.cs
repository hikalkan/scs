using System;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.SslScs.Authentication;
using Hik.Communication.SslScsServices.Service;
using PhoneBookCommonLib;

/* This is a simple phone book server application that runs on SCS framework.
 */

namespace PhoneBookServer
{
    class Program
    {
        static void Main()
        {
            var serverPublicPrivateKeys =
                new X509Certificate(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\privateKey.pfx",
                    "123456789");
            //Create a Scs Service application that runs on 10048 TCP port.
           // var server = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(10048));
            var server = SslScsServiceBuilder.CreateSslService(new ScsTcpEndPoint(10048)
           ,serverPublicPrivateKeys
           ,null
           ,SslScsAuthMode.ServerAuth);

            //Add Phone Book Service to service application
            server.AddService<IPhoneBookService, PhoneBookService>(new PhoneBookService());
            
            //Start server
            server.Start();

            //Wait user to stop server by pressing Enter
            Console.WriteLine("Phone Book Server started successfully. Press enter to stop...");
            Console.ReadLine();
            
            //Stop server
            server.Stop();
        }
    }
}
