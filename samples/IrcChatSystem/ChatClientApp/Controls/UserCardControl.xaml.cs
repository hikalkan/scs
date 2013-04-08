using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hik.Samples.Scs.IrcChat.Arguments;
using Hik.Samples.Scs.IrcChat.Client;

namespace Hik.Samples.Scs.IrcChat.Controls
{
    /// <summary>
    /// This control is used to show a User Card in right area of chat room.
    /// </summary>
    public partial class UserCardControl : UserControl
    {
        /// <summary>
        /// Gets/sets the of the user.
        /// </summary>
        public string UserNick
        {
            get { return lblNick.Content.ToString(); }
            set { lblNick.Content = value; }
        }

        /// <summary>
        /// Sets status of the user.
        /// </summary>
        public UserStatus UserStatus
        {
            get { return _userStatus; }
            set
            {
                _userStatus = value;
                RefreshStatusLabel();
            }
        }
        private UserStatus _userStatus;

        /// <summary>
        /// Sets avatar image of the user.
        /// </summary>
        public byte[] AvatarBytes
        {
            set
            {
                try
                {
                    ChangeAvatar(value);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Gets ImageSource property of user avatar.
        /// </summary>
        public ImageSource AvatarImageSource
        {
            get { return imgAvatar.Source; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserCardControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Refreshes status of user on label according to _userStatus.
        /// </summary>
        private void RefreshStatusLabel()
        {
            switch (_userStatus)
            {
                case UserStatus.Busy:
                    lblStatus.Content = "Busy";
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF4E4E"));
                    break;
                case UserStatus.Out:
                    lblStatus.Content = "Out";
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3179FE"));
                    break;
                default: //Default: Available
                    lblStatus.Content = "Available";
                    lblStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2BE400"));
                    break;
            }
        }

        /// <summary>
        /// Changes avatar image of the user.
        /// </summary>
        /// <param name="bytesOfAvatar">byte of avatar file</param>
        private void ChangeAvatar(byte[] bytesOfAvatar)
        {
            if (bytesOfAvatar == null)
            {
                var defaultAvatar = Path.Combine((Path.Combine(ClientHelper.GetCurrentDirectory(), @"Images\user_male.png")));
                imgAvatar.Source = new BitmapImage(new Uri(defaultAvatar));
                return;
            }

            //Save bytes into a temporary file
            var tempSavePath = Path.GetTempFileName();
            File.WriteAllBytes(tempSavePath, bytesOfAvatar);

            //Change avatar picture.
            imgAvatar.Source = new BitmapImage(new Uri(tempSavePath));
        }
    }
}
