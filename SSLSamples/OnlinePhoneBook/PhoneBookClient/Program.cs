using System;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.SslScs.Authentication;
using Hik.Communication.SslScsServices.Client;
using PhoneBookCommonLib;

/* This is a simple client application that uses phone book server.
 */

namespace PhoneBookClient
{
    class Program
    {
        static void Main()
        {
            var serverPublicKey =
                new X509Certificate2(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\publicKey.cer");
            //Create a client to connecto to phone book service on local server and 10048 TCP port.
            //var client = ScsServiceClientBuilder.CreateClient<IPhoneBookService>(new ScsTcpEndPoint("127.0.0.1", 10048));
            var client = SslScsServiceClientBuilder.CreateSslClient<IPhoneBookService>(new ScsTcpEndPoint("127.0.0.1", 10048)
            ,serverPublicKey
            ,"127.0.0.1"
            ,SslScsAuthMode.ServerAuth
            ,null,null);

            Console.WriteLine("Press enter to connect to phone book service...");
            Console.ReadLine();

            //Connect to the server
            client.Connect();

            var person1 = new PhoneBookRecord { Name = "Halil ibrahim", Phone = "5881112233" };
            var person2 = new PhoneBookRecord { Name = "John Nash", Phone = "58833322211" };

            //Add some persons
            client.ServiceProxy.AddPerson(person1);
            client.ServiceProxy.AddPerson(person2);

            //Search for a person
            var person = client.ServiceProxy.FindPerson("Halil");
            if (person != null)
            {
                Console.WriteLine("Person is found:");
                Console.WriteLine(person);
            }
            else
            {
                Console.WriteLine("Can not find person!");
            }

            Console.WriteLine();
            Console.WriteLine("Press enter to disconnect from phone book service...");
            Console.ReadLine();

            //Disconnect from server
            client.Disconnect();
        }
    }
}
