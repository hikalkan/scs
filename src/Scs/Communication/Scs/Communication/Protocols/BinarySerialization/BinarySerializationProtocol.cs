using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Hik.Communication.Scs.Communication.Messages;

namespace Hik.Communication.Scs.Communication.Protocols.BinarySerialization
{
    /// <summary>
    /// Default communication protocol between server and clients to send and receive a message.
    /// It uses .NET binary serialization to write and read messages.
    /// 
    /// A Message format:
    /// [Message Length (4 bytes)][Serialized Message Content]
    /// 
    /// If a message is serialized to byte array as N bytes, this protocol
    /// adds 4 bytes size information to head of the message bytes, so total length is (4 + N) bytes.
    /// 
    /// This class can be derived to change serializer (default: BinaryFormatter). To do this,
    /// SerializeMessage and DeserializeMessage methods must be overrided.
    /// </summary>
    public class BinarySerializationProtocol : IScsWireProtocol
    {
        #region Private fields

        /// <summary>
        /// Maximum length of a message.
        /// </summary>
        private const int MaxMessageLength = 128 * 1024 * 1024; //128 Megabytes.

        /// <summary>
        /// This MemoryStream object is used to collect receiving bytes to build messages.
        /// </summary>
        private MemoryStream _receiveMemoryStream;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of BinarySerializationProtocol.
        /// </summary>
        public BinarySerializationProtocol()
        {
            _receiveMemoryStream = new MemoryStream();
        }

        #endregion

        #region IScsWireProtocol implementation

        /// <summary>
        /// Serializes a message to a byte array to send to remote application.
        /// This method is synchronized. So, only one thread can call it concurrently.
        /// </summary>
        /// <param name="message">Message to be serialized</param>
        /// <exception cref="CommunicationException">Throws CommunicationException if message is bigger than maximum allowed message length.</exception>
        public byte[] GetBytes(IScsMessage message)
        {
            //Serialize the message to a byte array
            var serializedMessage = SerializeMessage(message); 
           
            //Check for message length
            var messageLength = serializedMessage.Length;
            if (messageLength > MaxMessageLength)
            {
                throw new CommunicationException("Message is too big (" + messageLength + " bytes). Max allowed length is " + MaxMessageLength + " bytes.");
            }

            //Create a byte array including the length of the message (4 bytes) and serialized message content
            var bytes = new byte[messageLength + 4];
            WriteInt32(bytes, 0, messageLength);
            Array.Copy(serializedMessage, 0, bytes, 4, messageLength);

            //Return serialized message by this protocol
            return bytes;
        }

        /// <summary>
        /// Builds messages from a byte array that is received from remote application.
        /// The Byte array may contain just a part of a message, the protocol must
        /// cumulate bytes to build messages.
        /// This method is synchronized. So, only one thread can call it concurrently.
        /// </summary>
        /// <param name="receivedBytes">Received bytes from remote application</param>
        /// <returns>
        /// List of messages.
        /// Protocol can generate more than one message from a byte array.
        /// Also, if received bytes are not sufficient to build a message, the protocol
        /// may return an empty list (and save bytes to combine with next method call).
        /// </returns>
        public IEnumerable<IScsMessage> CreateMessages(byte[] receivedBytes)
        {
            //Write all received bytes to the _receiveMemoryStream
            _receiveMemoryStream.Write(receivedBytes, 0, receivedBytes.Length);
            //Create a list to collect messages
            var messages = new List<IScsMessage>();
            //Read all available messages and add to messages collection
            while (ReadSingleMessage(messages)) { }
            //Return message list
            return messages;
        }

        /// <summary>
        /// This method is called when connection with remote application is reset (connection is renewing or first connecting).
        /// So, wire protocol must reset itself.
        /// </summary>
        public void Reset()
        {
            if (_receiveMemoryStream.Length > 0)
            {
                _receiveMemoryStream = new MemoryStream();
            }
        }

        #endregion

        #region Proptected virtual methods

        /// <summary>
        /// This method is used to serialize a IScsMessage to a byte array.
        /// This method can be overrided by derived classes to change serialization strategy.
        /// It is a couple with DeserializeMessage method and must be overrided together.
        /// </summary>
        /// <param name="message">Message to be serialized</param>
        /// <returns>
        /// Serialized message bytes.
        /// Does not include length of the message.
        /// </returns>
        protected virtual byte[] SerializeMessage(IScsMessage message)
        {
            using (var memoryStream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(memoryStream, message);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// This method is used to deserialize a IScsMessage from it's bytes.
        /// This method can be overrided by derived classes to change deserialization strategy.
        /// It is a couple with SerializeMessage method and must be overrided together.
        /// </summary>
        /// <param name="bytes">
        /// Bytes of message to be deserialized (does not include message length. It consist
        /// of a single whole message)
        /// </param>
        /// <returns>Deserialized message</returns>
        protected virtual IScsMessage DeserializeMessage(byte[] bytes)
        {
            //Create a MemoryStream to convert bytes to a stream
            using (var deserializeMemoryStream = new MemoryStream(bytes))
            {
                //Go to head of the stream
                deserializeMemoryStream.Position = 0;
                
                //Deserialize the message
                var binaryFormatter = new BinaryFormatter
                {
                    AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple,
                    Binder = new DeserializationAppDomainBinder()
                };
                
                //Return the deserialized message
                return (IScsMessage) binaryFormatter.Deserialize(deserializeMemoryStream);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method tries to read a single message and add to the messages collection. 
        /// </summary>
        /// <param name="messages">Messages collection to collect messages</param>
        /// <returns>
        /// Returns a boolean value indicates that if there is a need to re-call this method.
        /// </returns>
        /// <exception cref="CommunicationException">Throws CommunicationException if message is bigger than maximum allowed message length.</exception>
        private bool ReadSingleMessage(ICollection<IScsMessage> messages)
        {
            //Go to the begining of the stream
            _receiveMemoryStream.Position = 0;

            //If stream has less than 4 bytes, that means we can not even read length of the message
            //So, return false to wait more bytes from remore application.
            if (_receiveMemoryStream.Length < 4)
            {
                return false;
            }

            //Read length of the message
            var messageLength = ReadInt32(_receiveMemoryStream);
            if (messageLength > MaxMessageLength)
            {
                throw new Exception("Message is too big (" + messageLength + " bytes). Max allowed length is " + MaxMessageLength + " bytes.");
            }

            //If message is zero-length (It must not be but good approach to check it)
            if (messageLength == 0)
            {
                //if no more bytes, return immediately
                if (_receiveMemoryStream.Length == 4)
                {
                    _receiveMemoryStream = new MemoryStream(); //Clear the stream
                    return false;
                }

                //Create a new memory stream from current except first 4-bytes.
                var bytes = _receiveMemoryStream.ToArray();
                _receiveMemoryStream = new MemoryStream();
                _receiveMemoryStream.Write(bytes, 4, bytes.Length - 4);
                return true;
            }

            //If all bytes of the message is not received yet, return to wait more bytes
            if (_receiveMemoryStream.Length < (4 + messageLength))
            {
                _receiveMemoryStream.Position = _receiveMemoryStream.Length;
                return false;
            }

            //Read bytes of serialized message and deserialize it
            var serializedMessageBytes = ReadByteArray(_receiveMemoryStream, messageLength);
            messages.Add(DeserializeMessage(serializedMessageBytes));

            //Read remaining bytes to an array
            var remainingBytes = ReadByteArray(_receiveMemoryStream, (int)(_receiveMemoryStream.Length - (4 + messageLength)));

            //Re-create the receive memory stream and write remaining bytes
            _receiveMemoryStream = new MemoryStream();
            _receiveMemoryStream.Write(remainingBytes, 0, remainingBytes.Length);
            
            //Return true to re-call this method to try to read next message
            return (remainingBytes.Length > 4);
        }

        /// <summary>
        /// Writes a int value to a byte array from a starting index.
        /// </summary>
        /// <param name="buffer">Byte array to write int value</param>
        /// <param name="startIndex">Start index of byte array to write</param>
        /// <param name="number">An integer value to write</param>
        private static void WriteInt32(byte[] buffer, int startIndex, int number)
        {
            buffer[startIndex] = (byte)((number >> 24) & 0xFF);
            buffer[startIndex + 1] = (byte)((number >> 16) & 0xFF);
            buffer[startIndex + 2] = (byte)((number >> 8) & 0xFF);
            buffer[startIndex + 3] = (byte)((number) & 0xFF);
        }

        /// <summary>
        /// Deserializes and returns a serialized integer.
        /// </summary>
        /// <returns>Deserialized integer</returns>
        private static int ReadInt32(Stream stream)
        {
            var buffer = ReadByteArray(stream, 4);
            return ((buffer[0] << 24) |
                    (buffer[1] << 16) |
                    (buffer[2] << 8) |
                    (buffer[3])
                   );
        }

        /// <summary>
        /// Reads a byte array with specified length.
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="length">Length of the byte array to read</param>
        /// <returns>Read byte array</returns>
        /// <exception cref="EndOfStreamException">Throws EndOfStreamException if can not read from stream.</exception>
        private static byte[] ReadByteArray(Stream stream, int length)
        {
            var buffer = new byte[length];
            var totalRead = 0;
            while (totalRead < length)
            {
                var read = stream.Read(buffer, totalRead, length - totalRead);
                if (read <= 0)
                {
                    throw new EndOfStreamException("Can not read from stream! Input stream is closed.");
                }

                totalRead += read;
            }

            return buffer;
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// This class is used in deserializing to allow deserializing objects that are defined
        /// in assemlies that are load in runtime (like PlugIns).
        /// </summary>
        protected sealed class DeserializationAppDomainBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                var toAssemblyName = assemblyName.Split(',')[0];
                return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        where assembly.FullName.Split(',')[0] == toAssemblyName
                        select assembly.GetType(typeName)).FirstOrDefault();
            }
        }

        #endregion
    }
}
