using System;
using System.Security.Cryptography.X509Certificates;
using CalculatorCommonLib;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;
using Hik.Communication.SslScs.Authentication;
using Hik.Communication.SslScsServices.Service;

namespace CalculatorServer
{
    class Program
    {
        static void Main()
        {
            var serverPublicPrivateKeys =
                new X509Certificate(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\privateKey.pfx",
                    "123456789");
            //Create a service application that runs on 10083 TCP port
            //var serviceApplication = ScsServiceBuilder.CreateService(new ScsTcpEndPoint(10083));
            var serviceApplication = SslScsServiceBuilder.CreateSslService(new ScsTcpEndPoint(10083)
            ,serverPublicPrivateKeys
            ,null,
            SslScsAuthMode.ServerAuth);

            //Create a CalculatorService and add it to service application
            serviceApplication.AddService<ICalculatorService, CalculatorService>(new CalculatorService());
            
            //Start service application
            serviceApplication.Start();

            Console.WriteLine("Calculator service is started. Press enter to stop...");
            Console.ReadLine();

            //Stop service application
            serviceApplication.Stop();
        }
    }

    public class CalculatorService : ScsService, ICalculatorService
    {
        public int Add(int number1, int number2)
        {
            return number1 + number2;
        }

        public double Divide(double number1, double number2)
        {
            if(number2 == 0.0)
            {
                throw new DivideByZeroException("number2 can not be zero!");
            }

            return number1 / number2;
        }
    }
}
