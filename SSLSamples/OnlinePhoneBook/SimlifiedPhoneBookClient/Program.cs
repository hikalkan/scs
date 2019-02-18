using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.SslScs.Authentication;
using Hik.Communication.SslScsServices.Client;
using PhoneBookCommonLib;

/* This is the simplest client application that uses phone book server.
 * (Just 2 lines of code to connect to the server and call a method.
 */

namespace SimlifiedPhoneBookClient
{
    class Program
    {
        static void Main()
        {
            var serverPublicKey =
                new X509Certificate2(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\publicKey.cer");

            Console.WriteLine("Press any key to add person.");
            Console.ReadLine();

            //Create a client to connecto to phone book service on local server and 10048 TCP port.
            //var client = ScsServiceClientBuilder.CreateClient<IPhoneBookService>(new ScsTcpEndPoint("127.0.0.1", 10048));
            var client = SslScsServiceClientBuilder.CreateSslClient<IPhoneBookService>(new ScsTcpEndPoint("127.0.0.1", 10048)
                , serverPublicKey
                , "127.0.0.1"
                , SslScsAuthMode.ServerAuth
                , null, null);

            //Directly call a method (it automatically connects, calls and disconnects)
            client.ServiceProxy.AddPerson(new PhoneBookRecord { Name = "Halil ibrahim", Phone = "5881112233" });

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
