using System.IO;
using System.Media;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hik.Samples.Scs.IrcChat.Client
{
    /// <summary>
    /// This class includes come helper methods that are using in chat client.
    /// </summary>
    public static class ClientHelper
    {
        /// <summary>
        /// Gets the directory of executing assembly.
        /// </summary>
        /// <returns>Current directory</returns>
        public static string GetCurrentDirectory()
        {
            return (new FileInfo(Assembly.GetExecutingAssembly().Location)).Directory.FullName;
        }

        /// <summary>
        /// Gets the size of a file as bytes
        /// </summary>
        /// <param name="filePath">Path of file</param>
        /// <returns>Size of file</returns>
        public static long GetFileSize(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.Open))
            {
                var lengthOfFile = file.Length;
                file.Close();
                return lengthOfFile;
            }
        }

        /// <summary>
        /// Serializes an object and writes it to a file.
        /// Uses .NET binary serialization.
        /// </summary>
        /// <param name="obj">object to be serialized</param>
        /// <param name="filePath">Path of file to serialize</param>
        /// <returns>bytes of object</returns>
        public static void SerializeObjectToFile(object obj, string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Create))
            {
                new BinaryFormatter().Serialize(file, obj);
                file.Flush();
            }
        }

        /// <summary>
        /// Deserializes an object from a file.
        /// Uses .NET binary deserialization.
        /// </summary>
        /// <param name="filePath">Path of file to deserialize</param>
        /// <returns>deserialized object</returns>
        public static object DeserializeObjectFromFile(string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return new BinaryFormatter().Deserialize(file);
            }
        }

        /// <summary>
        /// Plays incoming message sound (if sound is on).
        /// </summary>
        public static void PlayIncomingMessageSound()
        {
            if (!UserPreferences.Current.IsSoundOn)
            {
                return;
            }

            try
            {
                var filePath = Path.Combine(GetCurrentDirectory(), @"Sounds\incoming_message.wav");
                if (!File.Exists(filePath))
                {
                    return;
                }

                new SoundPlayer(filePath).Play();
            }
            catch
            {

            }
        }
    }
}
