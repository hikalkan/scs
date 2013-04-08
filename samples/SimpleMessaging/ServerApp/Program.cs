using System;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;

/* This program is build to demonstrate a server application that listens incoming
 * client connections and reply messages.
 */

namespace ServerApp
{
    class Program
    {
        static void Main()
        {
            //Create a server that listens 10085 TCP port for incoming connections
            var server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(10085));

            //Register events of the server to be informed about clients
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            server.Start(); //Start the server

            Console.WriteLine("Server is started successfully. Press enter to stop...");
            Console.ReadLine(); //Wait user to press enter

            server.Stop(); //Stop the server
        }

        static void Server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            Console.WriteLine("A new client is connected. Client Id = " + e.Client.ClientId);

            //Register to MessageReceived event to receive messages from new client
            e.Client.MessageReceived += Client_MessageReceived;
        }

        static void Server_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            Console.WriteLine("A client is disconnected! Client Id = " + e.Client.ClientId);
        }

        static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            var message = e.Message as ScsTextMessage; //Server only accepts text messages
            if (message == null)
            {
                return;
            }

            //Get a reference to the client
            var client = (IScsServerClient)sender; 

            Console.WriteLine("Client sent a message: " + message.Text +
                              " (Cliend Id = " + client.ClientId + ")");

            //Send reply message to the client
            client.SendMessage(
                new ScsTextMessage(
                    "Hello client. I got your message (" + message.Text + ")",
                    message.MessageId //Set first message's id as replied message id
                    ));
        }
    }
}
