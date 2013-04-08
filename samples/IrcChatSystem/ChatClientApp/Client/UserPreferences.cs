using System;
using System.IO;
using Hik.Samples.Scs.IrcChat.Arguments;

namespace Hik.Samples.Scs.IrcChat.Client
{
    /// <summary>
    /// This class is used to save and load preferences of the user.
    /// </summary>
    [Serializable]
    internal class UserPreferences
    {
        /// <summary>
        /// Gets the singleton instance of this class.
        /// </summary>
        public static UserPreferences Current
        {
            get
            {
                if (_current == null)
                {
                    lock (SyncObj)
                    {
                        if (_current == null)
                        {
                            _current = LoadPreferences();
                        }
                    }
                }

                return _current;
            }
        }

        /// <summary>
        /// Nick of the user.
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// Path of user's avatar file.
        /// </summary>
        public string AvatarFile { get; set; }

        /// <summary>
        /// Sound preference of user.
        /// True if sound is on.
        /// </summary>
        public bool IsSoundOn { get; set; }

        /// <summary>
        /// Ip address of the chat server.
        /// </summary>
        public string ServerIpAddress { get; set; }

        /// <summary>
        /// TCP port of the chat server.
        /// </summary>
        public int ServerTcpPort { get; set; }
        
        /// <summary>
        /// Text style of user.
        /// </summary>
        public MessageTextStyle TextStyle { get; private set; }

        /// <summary>
        /// The singleton instance of this class.
        /// </summary>
        private static UserPreferences _current;

        /// <summary>
        /// Used to synronize threads while creating singleton object.
        /// </summary>
        private static readonly object SyncObj = new object();

        /// <summary>
        /// Constructor.
        /// </summary>
        private UserPreferences()
        {
            IsSoundOn = true;
            TextStyle = new MessageTextStyle();
        }

        /// <summary>
        /// Saves preferences to the disc.
        /// </summary>
        public void Save()
        {
            try
            {
                ClientHelper.SerializeObjectToFile(
                    this, 
                    Path.Combine(ClientHelper.GetCurrentDirectory(), "Preferences.bin")
                    );
            }
            catch
            {
                
            }
        }

        /// <summary>
        /// Load last preferences from the disc.
        /// </summary>
        /// <returns>Last user preferences (or default values if not found)</returns>
        private static UserPreferences LoadPreferences()
        {
            try
            {
                var preferenceFile = Path.Combine(ClientHelper.GetCurrentDirectory(), "Preferences.bin");
                if (File.Exists(preferenceFile))
                {
                    return (UserPreferences)ClientHelper.DeserializeObjectFromFile(preferenceFile);
                }
            }
            catch
            {

            }

            return CreateDefault();
        }

        /// <summary>
        /// Creates a default-valued instance of this class.
        /// </summary>
        /// <returns>UserPreferences object with default values</returns>
        private static UserPreferences CreateDefault()
        {
            return new UserPreferences
                   {
                       Nick = "User Nick",
                       AvatarFile = Path.Combine(ClientHelper.GetCurrentDirectory(), @"Images\user_male.png"),
                       ServerIpAddress = "127.0.0.1",
                       ServerTcpPort = 10048
                   };
        }
    }
}
