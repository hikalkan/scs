using System;
using System.Security.Cryptography.X509Certificates;
using CommonLib;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.SslScs.Client;

/* This application is build to demonstrate a CUSTOM WIRE PROTOCOL usage with SCS framework.
 * This client application connects to a server and sends a message using MyWireProtocol class.
 */

namespace ClientApp
{
    class Program
    {
        static void Main()
        {
            var serverPublicKey =
                new X509Certificate2(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\publicKey.cer");

            Console.WriteLine("Press enter to connect to server and say Hello world!");
            Console.ReadLine();

            //using (var client = ScsClientFactory.CreateClient(new ScsTcpEndPoint("127.0.0.1", 10033)))
            using (var client = SslScsClientFactory.CreateSslClient(new ScsTcpEndPoint("127.0.0.1", 10033)
            , serverPublicKey
            , "127.0.0.1"))
            {
                client.WireProtocol = new MyWireProtocol(); //Set custom wire protocol!

                client.Connect();
                client.SendMessage(new ScsTextMessage("Hello world!"));

                Console.WriteLine("Press enter to disconnect from server");
                Console.ReadLine();
            }
        }
    }
}
