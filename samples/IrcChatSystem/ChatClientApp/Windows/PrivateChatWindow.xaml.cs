using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Hik.Samples.Scs.IrcChat.Arguments;
using Hik.Samples.Scs.IrcChat.Client;
using Hik.Samples.Scs.IrcChat.Controls;

namespace Hik.Samples.Scs.IrcChat.Windows
{
    /// <summary>
    /// Interaction logic for PrivateChatWindow.xaml
    /// </summary>
    public partial class PrivateChatWindow : Window, IMessagingAreaContainer
    {
        #region Public properties

        /// <summary>
        /// Nick of the current user.
        /// </summary>
        public string CurrentUserNick { set; get; }

        /// <summary>
        /// Sets the status of the current user.
        /// </summary>
        public UserStatus CurrentUserStatus
        {
            set { SetStatus(lblCurrentUserStatus, value); }
        }

        /// <summary>
        /// Sets the avatar picture of current user.
        /// </summary>
        public ImageSource CurrentUserAvatar
        {
            set { imgCurrentUserAvatar.Source = value; }
        }

        /// <summary>
        /// Gets/Sets the nick of the remote user.
        /// </summary>
        public string RemoteUserNick
        {
            set
            {
                _remoteUserNick = value;
                Title = _remoteUserNick;
            }

            get { return _remoteUserNick; }
        }
        private string _remoteUserNick;

        /// <summary>
        /// Sets the status of the remote user.
        /// </summary>
        public UserStatus RemoteUserStatus
        {
            set
            {
                _remoteUserStatus = value;
                SetStatus(lblRemoteUserStatus, _remoteUserStatus);
            }
        }
        private UserStatus _remoteUserStatus;

        /// <summary>
        /// Sets the avatar picture of the remote user.
        /// </summary>
        public ImageSource RemoteUserAvatar
        {
            set { imgRemoteUserAvatar.Source = value; }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to chat controller to send private messages to remote user.
        /// </summary>
        private readonly IChatController _controller;

        /// <summary>
        /// WindowInteropHelper object that is used to get a Handle to this window.
        /// </summary>
        private readonly WindowInteropHelper _windowInteropHelper;

        #endregion

        #region Contructor and initializing methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="controller">Reference to chat controller to send private messages to remote user</param>
        public PrivateChatWindow(IChatController controller)
        {
            _controller = controller;
            _windowInteropHelper = new WindowInteropHelper(this);
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            MessageHistory.MessagingAreaContainer = this;
            MessageHistory.IsTextStyleChangingEnabled = false;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// This method is used to add a new message to message history of that window.
        /// </summary>
        /// <param name="message">Message</param>
        public void MessageReceived(ChatMessage message)
        {
            MessageHistory.MessageReceived(_remoteUserNick, message);
            if (!IsActive)
            {
                //Flash taskbar button if this window is not active
                WindowsHelper.FlashWindow(_windowInteropHelper.Handle, WindowsHelper.FlashWindowFlags.FLASHW_TRAY, 1, 1000);
                ClientHelper.PlayIncomingMessageSound();
            }
        }

        /// <summary>
        /// This method is called when remote user has logged off.
        /// </summary>
        public void UserLoggedOut()
        {
            Title = RemoteUserNick + " - offline";
            MessageHistory.IsReadOnly = true;
            lblRemoteUserStatus.Content = "Offline";
            lblRemoteUserStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDDDDDD"));
        }

        /// <summary>
        /// This method is called when remote user has logged in again.
        /// </summary>
        public void UserLoggedIn()
        {
            Title = RemoteUserNick;
            MessageHistory.IsReadOnly = false;
        }
        
        /// <summary>
        /// This method is called by MessagingAreaControl to send messages.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        public void SendMessage(ChatMessage message)
        {
            _controller.SendPrivateMessage(RemoteUserNick, message);
            MessageHistory.MessageReceived(CurrentUserNick, message);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sets status of a user to a label.
        /// </summary>
        /// <param name="statusLabel">Label to show user status</param>
        /// <param name="status">New status of the user</param>
        private static void SetStatus(Label statusLabel, UserStatus status)
        {
            switch (status)
            {
                case UserStatus.Busy:
                    statusLabel.Content = "Busy";
                    statusLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF4E4E"));
                    break;
                case UserStatus.Out:
                    statusLabel.Content = "Out";
                    statusLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3179FE"));
                    break;
                default: //Default: Available
                    statusLabel.Content = "Available";
                    statusLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2BE400"));
                    break;
            }
        }

        #endregion
    }
}
