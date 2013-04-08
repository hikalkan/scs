using System.Windows;
using Hik.Samples.Scs.IrcChat.Client;
using Hik.Samples.Scs.IrcChat.Windows;

namespace Hik.Samples.Scs.IrcChat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class ChatClientApplication : Application
    {
        public ChatClientApplication()
        {
            Startup += AppStartUp;
        }

        static void AppStartUp(object sender, StartupEventArgs e)
        {
            var controller = new ChatController();
            var mainWindow = new MainWindow(controller);
            controller.ChatRoom = mainWindow;
            controller.LoginForm = mainWindow;
            mainWindow.Show();
        }
    }
}
