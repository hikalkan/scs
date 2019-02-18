using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CalculatorCommonLib
{
    public class Consts
    {
        public const int MethodCallCount = 100000;

        private static X509Certificate _serverPublicPrivateKey;
        public static X509Certificate ServerPublicPrivateKey => _serverPublicPrivateKey
                                                                ?? (_serverPublicPrivateKey = new X509Certificate(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\privateKey.pfx",
                                                                    "123456789"));

        private static X509Certificate2 _serverPublicKey;
        public static X509Certificate2 ServerPublicKey => _serverPublicKey
                                                          ?? (_serverPublicKey = new X509Certificate2(@"C:\Users\node\Desktop\scs\SSLSamples\CertificateFiles\Server\publicKey.cer"));
    }
}
