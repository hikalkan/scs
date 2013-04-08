using System;
using System.Net;
using System.Net.Sockets;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to simplify TCP socket operations.
    /// </summary>
    internal static class TcpHelper
    {
        /// <summary>
        /// This code is used to connect to a TCP socket with timeout option.
        /// </summary>
        /// <param name="endPoint">IP endpoint of remote server</param>
        /// <param name="timeoutMs">Timeout to wait until connect</param>
        /// <returns>Socket object connected to server</returns>
        /// <exception cref="SocketException">Throws SocketException if can not connect.</exception>
        /// <exception cref="TimeoutException">Throws TimeoutException if can not connect within specified timeoutMs</exception>
        public static Socket ConnectToServer(EndPoint endPoint, int timeoutMs)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Blocking = false;
                socket.Connect(endPoint);
                socket.Blocking = true;
                return socket;
            }
            catch (SocketException socketException)
            {
                if (socketException.ErrorCode != 10035)
                {
                    socket.Close();
                    throw;
                }

                if (!socket.Poll(timeoutMs * 1000, SelectMode.SelectWrite))
                {
                    socket.Close();
                    throw new TimeoutException("The host failed to connect. Timeout occured.");
                }

                socket.Blocking = true;
                return socket;
            }
        }
    }
}
