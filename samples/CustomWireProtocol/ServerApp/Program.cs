using System;
using CommonLib;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Server;

/* This application is build to demonstrate a CUSTOM WIRE PROTOCOL usage with SCS framework.
 * This server application listens incoming messages from client applications using MyWireProtocol class.
 */

namespace ServerApp
{
    public class Program
    {
        static void Main()
        {
            var server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(10033));
            
            server.WireProtocolFactory = new MyWireProtocolFactory(); //Set custom wire protocol factory!
            server.ClientConnected += server_ClientConnected;

            server.Start();

            Console.WriteLine("Press enter to stop server");
            Console.ReadLine();

            server.Stop();
        }

        static void server_ClientConnected(object sender, ServerClientEventArgs e)
        {
            Console.WriteLine("A new client is connected. Address: " + e.Client.RemoteEndPoint);
            e.Client.MessageReceived += Client_MessageReceived;
        }

        static void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine("A client sent a message: " + ((ScsTextMessage) e.Message).Text);
        }
    }
}
