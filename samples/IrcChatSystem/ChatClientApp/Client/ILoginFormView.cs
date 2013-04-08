using Hik.Samples.Scs.IrcChat.Arguments;

namespace Hik.Samples.Scs.IrcChat.Client
{
    /// <summary>
    /// This interface is used to interact with login form by ChatController.
    /// ChatController gets user informations over this interface.
    /// </summary>
    public interface ILoginFormView
    {
        /// <summary>
        /// IP address of server to be connected.
        /// </summary>
        string ServerIpAddress { get; }

        /// <summary>
        /// TCP Port number of server to be connected.
        /// </summary>
        int ServerTcpPort { get; }

        /// <summary>
        /// User Login informations to be used while logging on to the server.
        /// </summary>
        UserInfo CurrentUserInfo { get; }
    }
}