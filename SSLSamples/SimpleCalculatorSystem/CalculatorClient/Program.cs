using System;
using System.Security.Cryptography.X509Certificates;
using CalculatorCommonLib;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.SslScs.Authentication;
using Hik.Communication.SslScsServices.Client;

namespace CalculatorClient
{
    class Program
    {
        static void Main()
        {
            var serverPublicKey =
                new X509Certificate2(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\publicKey.cer");
            Console.WriteLine("Press enter to connect to server and call methods...");
            Console.ReadLine();

            //Create a client that can call methods of Calculator Service that is running on local computer and 10083 TCP port
            //Since IScsServiceClient is IDisposible, it closes connection at the end of the using block
            //using (var client = ScsServiceClientBuilder.CreateClient<ICalculatorService>(new ScsTcpEndPoint("127.0.0.1", 10083)))
            using (var client = SslScsServiceClientBuilder.CreateSslClient<ICalculatorService>(new ScsTcpEndPoint("127.0.0.1", 10083)
            ,serverPublicKey,"127.0.0.1",SslScsAuthMode.ServerAuth,null,null))
            {
                //Connect to the server
                client.Connect();

                //Call a remote method of server
                var division = client.ServiceProxy.Divide(42, 3);

                //Write the result to the screen
                Console.WriteLine("Result: " + division);
            }

            Console.WriteLine("Press enter to stop client application");
            Console.ReadLine();
        }
    }
}
