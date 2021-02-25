using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.SslScs.Client;

/* This program is build to demonstrate a client application that connects to a server
 * and sends/receives messages in basic way of SCS framework.
 */

namespace ClientApp
{
    class Program
    {
        static void Main()
        {
            var serverPublicKey =new X509Certificate2(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\publicKey.cer");
            //Create a client object to connect a server on 127.0.0.1 (local) IP and listens 10085 TCP port
            //var client = ScsClientFactory.CreateClient(new ScsTcpEndPoint("127.0.0.1", 10085));
            var client = SslScsClientFactory.CreateSslClient(new ScsTcpEndPoint("127.0.0.1", 10085)
            ,serverPublicKey
            ,"127.0.0.1"
            );

            //Register to MessageReceived event to receive messages from server.
            client.MessageReceived += Client_MessageReceived;
            
            Console.WriteLine("Press enter to connect to the server...");
            Console.ReadLine(); //Wait user to press enter

            client.Connect(); //Connect to the server
            
            Console.Write("Write some message to be sent to server: ");
            var messageText = Console.ReadLine(); //Get a message from user
            
            //Send message to the server
            client.SendMessage(new ScsTextMessage(messageText));                
            
            Console.WriteLine("Press enter to disconnect from server...");
            Console.ReadLine(); //Wait user to press enter
            
            client.Disconnect(); //Close connection to server
        }

        static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            //Client only accepts text messages
            var message = e.Message as ScsTextMessage;
            if (message == null)
            {
                return;
            }

            Console.WriteLine("Server sent a message: " + message.Text);
        }
    }
}