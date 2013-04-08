using System;
using CommonLib;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Messengers;

namespace ClientApp
{
    class DuplexClientDefaultProtocolSynchronized
    {
        public static void Run()
        {
            Console.WriteLine("Press enter to connect to server and send " + Consts.MessageCount + " messages.");
            Console.ReadLine();

            using (var client = ScsClientFactory.CreateClient(new ScsTcpEndPoint("127.0.0.1", 10033)))
            {
                using (var synchronizedMessenger = new SynchronizedMessenger<IScsClient>(client))
                {
                    synchronizedMessenger.Start();
                    client.Connect();

                    for (var i = 0; i < Consts.MessageCount; i++)
                    {
                        synchronizedMessenger.SendMessage(new ScsTextMessage("Hello from client!"));
                        var reply = synchronizedMessenger.ReceiveMessage<ScsTextMessage>();
                    }
                }

                Console.WriteLine("Press enter to disconnect from server");
                Console.ReadLine();
            }
        }
    }
}
