using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Tcp;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using System.Net;

namespace Hik.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to communicate with server over TCP/IP protocol.
    /// </summary>
    internal class ScsTcpClient : ScsClientBase
    {
        /// <summary>
        /// The endpoint address of the server.
        /// </summary>
        private readonly ScsTcpEndPoint _serverEndPoint;

        /// <summary>
        /// Creates a new ScsTcpClient object.
        /// </summary>
        /// <param name="serverEndPoint">The endpoint address to connect to the server</param>
        public ScsTcpClient(ScsTcpEndPoint serverEndPoint)
        {
            _serverEndPoint = serverEndPoint;
        }

        /// <summary>
        /// Creates a communication channel using ServerIpAddress and ServerPort.
        /// </summary>
        /// <returns>Ready communication channel to communicate</returns>
        protected override ICommunicationChannel CreateCommunicationChannel()
        {

            EndPoint endpoint = null;

            if (IsStringIp(_serverEndPoint.IpAddress))
            {
                endpoint = new IPEndPoint(IPAddress.Parse(_serverEndPoint.IpAddress), _serverEndPoint.TcpPort);
            }
            else
            {
                endpoint = new DnsEndPoint(_serverEndPoint.IpAddress, _serverEndPoint.TcpPort);
            }

            return new TcpCommunicationChannel(
                TcpHelper.ConnectToServer(
                    endpoint,
                    ConnectTimeout
                    ));


        }

        private bool IsStringIp(string address)
        {
            IPAddress ipAddress = null;
            return IPAddress.TryParse(address, out ipAddress);
        }

    }
}
