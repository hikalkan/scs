using System.Text;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols.BinarySerialization;

namespace CommonLib
{
    /// <summary>
    /// This class is a sample custom wire protocol to use as wire protocol in SCS framework.
    /// It extends BinarySerializationProtocol.
    /// It is used just to send/receive ScsTextMessage messages.
    /// 
    /// Since BinarySerializationProtocol automatically writes message length to the beggining
    /// of the message, a message format of this class is:
    /// 
    /// [Message length (4 bytes)][UTF-8 encoded text (N bytes)]
    /// 
    /// So, total length of the message = (N + 4) bytes;
    /// </summary>
    public class MyWireProtocol : BinarySerializationProtocol
    {
        protected override byte[] SerializeMessage(IScsMessage message)
        {
            return Encoding.UTF8.GetBytes(((ScsTextMessage)message).Text);
        }

        protected override IScsMessage DeserializeMessage(byte[] bytes)
        {
            //Decode UTF8 encoded text and create a ScsTextMessage object
            return new ScsTextMessage(Encoding.UTF8.GetString(bytes));
        }
    }
}
