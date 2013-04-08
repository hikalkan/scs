using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hik.Samples.Scs.IrcChat.Arguments;
using Hik.Samples.Scs.IrcChat.Client;
using Hik.Samples.Scs.IrcChat.Windows;

namespace Hik.Samples.Scs.IrcChat.Controls
{
    /// <summary>
    /// This control is used to show incoming messages and write new messages.
    /// </summary>
    public partial class MessagingAreaControl : UserControl
    {
        #region Public Properties

        /// <summary>
        /// Reference to the container window of this control.
        /// </summary>
        public IMessagingAreaContainer MessagingAreaContainer { get; set; }

        /// <summary>
        /// This property is used to set messaging area is read only or not.
        /// </summary>
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                btnSendMessage.IsEnabled = !_isReadOnly;
                txtWriteMessage.IsEnabled = !_isReadOnly;
            }
        }
        private bool _isReadOnly;

        private void SetTextStyleControlsVisibility()
        {
            if (!IsInitialized)
            {
                return;
            }
            spTextStyleChanging.Visibility = _isTextStyleChangingEnabled
                                                 ? Visibility.Visible
                                                 : Visibility.Collapsed;
        }

        /// <summary>
        /// This property is used to hide or show text style changing controls.
        /// </summary>
        public bool IsTextStyleChangingEnabled
        {
            get { return IsTextStyleChangingEnabled; }
            set
            {
                if (_isTextStyleChangingEnabled == value)
                {
                    return;
                }

                _isTextStyleChangingEnabled = value;
                SetTextStyleControlsVisibility(); ;
            }
        }
        private bool _isTextStyleChangingEnabled = true;

        #endregion

        #region Constructor and initializing methods

        /// <summary>
        /// Constructor.
        /// </summary>
        public MessagingAreaControl()
        {
            _userPreferences = UserPreferences.Current;
            InitializeComponent();
            InitializeControls();
            InitializeUserPreferences();
        }

        /// <summary>
        /// Initializes some controls.
        /// </summary>
        private void InitializeControls()
        {
            txtMessageHistory.IsReadOnly = true;
        }

        /// <summary>
        /// Gets user preferences and initializes controls.
        /// </summary>
        private void InitializeUserPreferences()
        {
            lblTextBold.FontWeight = _userPreferences.TextStyle.IsBold ? FontWeights.Bold : FontWeights.Normal;
            lblTextColor.FontWeight = lblTextBold.FontWeight;
            lblTextItalic.FontWeight = lblTextBold.FontWeight;
            txtWriteMessage.FontWeight = lblTextBold.FontWeight;

            lblTextItalic.FontStyle = _userPreferences.TextStyle.IsItalic ? FontStyles.Italic : FontStyles.Normal;
            lblTextColor.FontStyle = lblTextItalic.FontStyle;
            lblTextBold.FontStyle = lblTextItalic.FontStyle;
            txtWriteMessage.FontStyle = lblTextItalic.FontStyle;

            lblTextColor.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        _userPreferences.TextStyle.TextColor.Red, _userPreferences.TextStyle.TextColor.Green, _userPreferences.TextStyle.TextColor.Blue
                        ));
            lblTextBold.Foreground = lblTextColor.Foreground;
            lblTextItalic.Foreground = lblTextColor.Foreground;
            txtWriteMessage.Foreground = lblTextColor.Foreground;

            for (var i = 0; i < cmbTextFont.Items.Count; i++)
            {
                if (((string)((ComboBoxItem)cmbTextFont.Items[i]).Content) == _userPreferences.TextStyle.FontFamily)
                {
                    cmbTextFont.SelectedIndex = i;
                    break;
                }
            }

            var textSizeAsString = _userPreferences.TextStyle.TextSize.ToString();
            for (var i = 0; i < cmbTextSize.Items.Count; i++)
            {
                if (((string)((ComboBoxItem)cmbTextSize.Items[i]).Content) == textSizeAsString)
                {
                    cmbTextSize.SelectedIndex = i;
                    break;
                }
            }

            txtWriteMessage.FontFamily = new FontFamily(_userPreferences.TextStyle.FontFamily);
            txtWriteMessage.FontSize = _userPreferences.TextStyle.TextSize;

            RefreshSoundPicture();
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to the user's text style.
        /// </summary>
        private readonly UserPreferences _userPreferences;

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a new message to message history.
        /// </summary>
        /// <param name="nick">Nick of sender</param>
        /// <param name="message">Message</param>
        public void MessageReceived(string nick, ChatMessage message)
        {
            //Create a new paragraph to write new message
            var messageParagraph = new Paragraph();

            //Set message as Bold if needed
            if (message.TextStyle.IsBold)
            {
                messageParagraph.FontWeight = FontWeights.Bold;
            }

            //Set message as Italic if needed
            if (message.TextStyle.IsItalic)
            {
                messageParagraph.FontStyle = FontStyles.Italic;
            }

            //Set message font if needed
            if (!string.IsNullOrEmpty(message.TextStyle.FontFamily))
            {
                try
                {
                    messageParagraph.FontFamily = new FontFamily(message.TextStyle.FontFamily);
                }
                catch
                {

                }
            }

            //Set message text size if needed
            if (message.TextStyle.TextSize > 0)
            {
                messageParagraph.FontSize = message.TextStyle.TextSize;
            }

            //Set message color if needed
            if (message.TextStyle.TextColor != null)
            {
                messageParagraph.Foreground =
                    new SolidColorBrush(
                        new Color
                        {
                            A = 255,
                            R = message.TextStyle.TextColor.Red,
                            G = message.TextStyle.TextColor.Green,
                            B = message.TextStyle.TextColor.Blue
                        });
            }

            //Add message to paragraph
            messageParagraph.Inlines.Add(new Run(nick + ": " + message.MessageText));

            //Add new parapraph to message history
            txtMessageHistory.Document.Blocks.Add(messageParagraph);

            if (txtMessageHistory.Document.Blocks.Count > 1000)
            {
                txtMessageHistory.Document.Blocks.Remove(txtMessageHistory.Document.Blocks.FirstBlock);
            }

            txtMessageHistory.ScrollToEnd();
        }

        #endregion

        #region Private methods

        #region Sending message

        /// <summary>
        /// Handles Client event of Send button.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        /// <summary>
        /// Handles KeyDown event of txtWriteMessage textbox.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void txtWriteMessage_KeyDown(object sender, KeyEventArgs e)
        {
            //If user pressed to enter in message sending textbox, send message..
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        /// <summary>
        /// Sends a message to the room.
        /// </summary>
        private void SendMessage()
        {
            string messageText = new TextRange(txtWriteMessage.Document.ContentStart, txtWriteMessage.Document.ContentEnd).Text.Trim();
            if (string.IsNullOrEmpty(messageText) || MessagingAreaContainer == null)
            {
                return;
            }

            try
            {
                MessagingAreaContainer.SendMessage(
                    new ChatMessage(
                        messageText,
                        _userPreferences.TextStyle
                        ));
                txtWriteMessage.Document.Blocks.Clear();
                txtWriteMessage.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can not send message to the server. Error Detail: " + ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Changing / getting text styles

        /// <summary>
        /// Handles MouseLeftButtonUp of txtTextColor and opens a text color picker dialog
        /// to select text color.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void lblTextColor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var colorPicker = new TextColorPicker();
            if (colorPicker.ShowDialog() == true)
            {
                _userPreferences.TextStyle.TextColor.Red = colorPicker.SelectedColor.R;
                _userPreferences.TextStyle.TextColor.Green = colorPicker.SelectedColor.G;
                _userPreferences.TextStyle.TextColor.Blue = colorPicker.SelectedColor.B;

                lblTextColor.Foreground = new SolidColorBrush(colorPicker.SelectedColor);
                lblTextBold.Foreground = lblTextColor.Foreground;
                lblTextItalic.Foreground = lblTextColor.Foreground;
                txtWriteMessage.Foreground = lblTextColor.Foreground;
            }
        }

        /// <summary>
        /// Handles MouseLeftButtonUp event of lblTextBold to change Bold text option.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void lblTextBold_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lblTextBold.FontWeight = lblTextBold.FontWeight == FontWeights.Normal
                                         ? FontWeights.Bold
                                         : FontWeights.Normal;

            _userPreferences.TextStyle.IsBold = (lblTextBold.FontWeight == FontWeights.Bold);
            lblTextColor.FontWeight = lblTextBold.FontWeight;
            lblTextItalic.FontWeight = lblTextBold.FontWeight;
            txtWriteMessage.FontWeight = lblTextBold.FontWeight;
        }

        /// <summary>
        /// Handles MouseLeftButtonUp event of lblTextItalic to change Italic text option.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void lblTextItalic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lblTextItalic.FontStyle = lblTextItalic.FontStyle == FontStyles.Normal
                                          ? FontStyles.Italic
                                          : FontStyles.Normal;

            _userPreferences.TextStyle.IsItalic = (lblTextItalic.FontStyle == FontStyles.Italic);
            lblTextColor.FontStyle = lblTextItalic.FontStyle;
            lblTextBold.FontStyle = lblTextItalic.FontStyle;
            txtWriteMessage.FontStyle = lblTextItalic.FontStyle;
        }

        /// <summary>
        /// Handles SelectionChanged event of text font combobox.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void cmbTextFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            var selectedFont = GetSelectedTextFontFamily();
            if (selectedFont == null)
            {
                return;
            }

            try
            {
                txtWriteMessage.FontFamily = new FontFamily(selectedFont);
                _userPreferences.TextStyle.FontFamily = selectedFont;
            }
            catch
            {

            }
        }

        /// <summary>
        /// Handles SelectionChanged event of text size combobox.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void cmbTextSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }

            var selectedTextSize = GetSelectedTextSize();
            txtWriteMessage.FontSize = selectedTextSize;
            _userPreferences.TextStyle.TextSize = selectedTextSize;
        }

        /// <summary>
        /// Handles MouseLeftButtonUp event of Sound image.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void imgSoundOnOff_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _userPreferences.IsSoundOn = !_userPreferences.IsSoundOn;
            RefreshSoundPicture();
        }

        /// <summary>
        /// Gets selected font family.
        /// </summary>
        /// <returns>Selected font family</returns>
        private string GetSelectedTextFontFamily()
        {
            if (cmbTextFont.SelectedIndex < 0)
            {
                return null;
            }

            var selectedItem = cmbTextFont.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                return null;
            }

            return selectedItem.Content as string;
        }

        /// <summary>
        /// Gets selected text size.
        /// </summary>
        /// <returns>Text size</returns>
        private int GetSelectedTextSize()
        {
            try
            {
                if (cmbTextFont.SelectedIndex < 0)
                {
                    return 12; //Default value
                }

                var selectedItem = cmbTextSize.SelectedItem as ComboBoxItem;
                if (selectedItem != null)
                {
                    return Convert.ToInt32(selectedItem.Content as string);
                }
            }
            catch
            {

            }

            return 12; //Default value
        }

        /// <summary>
        /// Refreshes sound image according to user preference.
        /// </summary>
        private void RefreshSoundPicture()
        {
            var imagePath = _userPreferences.IsSoundOn
                    ? Path.Combine(ClientHelper.GetCurrentDirectory(), @"Images\sound_on.png")
                    : Path.Combine(ClientHelper.GetCurrentDirectory(), @"Images\sound_off.png");
            try
            {
                imgSoundOnOff.Source = new BitmapImage(new Uri(imagePath));
            }
            catch
            {

            }
        }

        #endregion

        #endregion
    }
}
