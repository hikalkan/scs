using System;
using CalculatorCommonLib;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Client;
using System.Diagnostics;

namespace CalculatorClient
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Press enter to connect to server and call " + Consts.MethodCallCount + " methods...");
            Console.ReadLine();

            using (var client = ScsServiceClientBuilder.CreateClient<ICalculatorService>(new ScsTcpEndPoint("127.0.0.1", 10083)))
            {
                client.Connect();

                var stopwatch = Stopwatch.StartNew();
                for (var i = 0; i < Consts.MethodCallCount; i++)
                {
                    var division = client.ServiceProxy.Add(2, 3);
                }

                stopwatch.Stop();
                Console.WriteLine(Consts.MethodCallCount + " remote method call made in " + stopwatch.Elapsed.TotalMilliseconds.ToString("0.000") + " ms.");
            }

            Console.WriteLine("Press enter to stop client application");
            Console.ReadLine();
        }
    }
}
