using Hik.Communication.Scs.Communication.Protocols;

namespace CommonLib
{
    public class MyWireProtocolFactory : IScsWireProtocolFactory
    {
        public IScsWireProtocol CreateWireProtocol()
        {
            return new MyWireProtocol();
        }
    }
}
