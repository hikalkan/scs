using System;
using System.Security.Cryptography.X509Certificates;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.SslScs.Client;

/* This program is build to demonstrate a client application that connects to a server
 * and sends/receives messages in using SynchronizedMessenger.
 */

namespace SynchronizedClient
{
    class Program
    {
        static void Main()
        {
            var serverPublicKey = new X509Certificate2(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\publicKey.cer");
            Console.WriteLine("Press enter to connect to the server...");
            Console.ReadLine(); //Wait user to press enter

            //Create a client object to connect a server on 127.0.0.1 (local) IP and listens 10085 TCP port
           // using (var client = ScsClientFactory.CreateClient(new ScsTcpEndPoint("127.0.0.1", 10085)))
            using (var client = SslScsClientFactory.CreateSslClient(new ScsTcpEndPoint("127.0.0.1", 10085)
            ,serverPublicKey
            ,"127.0.0.1"))
            {
                //Create a SynchronizedMessenger that uses the client as internal messenger.
                using (var synchronizedMessenger = new SynchronizedMessenger<IScsClient>(client))
                {
                    synchronizedMessenger.Start(); //Start synchronized messenger messenger
                    client.Connect(); //Connect to the server

                    Console.Write("Write some message to be sent to server: ");
                    var messageText = Console.ReadLine(); //Get a message from user

                    //Send a message to the server
                    synchronizedMessenger.SendMessage(new ScsTextMessage(messageText));

                    //Receive a message from the server
                    var receivedMessage = synchronizedMessenger.ReceiveMessage<ScsTextMessage>();

                    Console.WriteLine("Response to message: " + (receivedMessage.Text));

                    Console.WriteLine("Press enter to disconnect from server...");
                    Console.ReadLine(); //Wait user to press enter
                }
            }
        }
    }
}
