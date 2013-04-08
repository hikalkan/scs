using System;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Messengers;

/* This program is build to demonstrate a client application that connects to a server
 * and sends/receives messages in using RequestReplyMessenger.
 */

namespace RequestReplyStyleClient
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Press enter to connect to the server...");
            Console.ReadLine(); //Wait user to press enter

            //Create a client object to connect a server on 127.0.0.1 (local) IP and listens 10085 TCP port
            using (var client = ScsClientFactory.CreateClient(new ScsTcpEndPoint("127.0.0.1", 10085)))
            {
                //Create a RequestReplyMessenger that uses the client as internal messenger.
                using (var requestReplyMessenger = new RequestReplyMessenger<IScsClient>(client))
                {
                    requestReplyMessenger.Start(); //Start request/reply messenger
                    client.Connect(); //Connect to the server

                    Console.Write("Write some message to be sent to server: ");
                    var messageText = Console.ReadLine(); //Get a message from user

                    //Send user message to the server and get response
                    var response = requestReplyMessenger.SendMessageAndWaitForResponse(new ScsTextMessage(messageText));
                    
                    Console.WriteLine("Response to message: " + ((ScsTextMessage) response).Text);

                    Console.WriteLine("Press enter to disconnect from server...");
                    Console.ReadLine(); //Wait user to press enter
                }
            }
        }
    }
}
