using System;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Client;
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
            Console.ReadLine();

            //Create a client to connecto to phone book service on local server and 10048 TCP port.
            var client = ScsServiceClientBuilder.CreateClient<IPhoneBookService>(new ScsTcpEndPoint("127.0.0.1", 10048));

            //Directly call a method (it automatically connects, calls and disconnects)
            client.ServiceProxy.AddPerson(new PhoneBookRecord { Name = "Halil ibrahim", Phone = "5881112233" });

            Console.ReadLine();
        }
    }
}
