using System;
using System.Net;
using System.Net.Sockets;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Client.Tcp;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Server.Tcp;

namespace Hik.Communication.Scs.Communication.EndPoints.Tcp
{
    /// <summary>
    /// Represents a TCP end point in SCS.
    /// </summary>
    public sealed class ScsTcpEndPoint : ScsEndPoint
    {
        #region Private fields

        private SocketInformation? _existingSocketInformation;

        #endregion

        ///<summary>
        /// IP address of the server.
        ///</summary>
        public IPAddress IpAddress { get; set; }

        ///<summary>
        /// Listening TCP Port for incoming connection requests on server.
        ///</summary>
        public int TcpPort { get; private set; }

        /// <summary>
        /// Creates a new ScsTcpEndPoint object with specified port number.
        /// </summary>
        /// <param name="tcpPort">Listening TCP Port for incoming connection requests on server</param>
        public ScsTcpEndPoint(int tcpPort)
        {
            TcpPort = tcpPort;
        }

        /// <summary>
        /// Creates a new ScsTcpEndPoint object with specified IP address and port number.
        /// </summary>
        /// <param name="ipAddress">IP address of the server</param>
        /// <param name="port">Listening TCP Port for incoming connection requests on server</param>
        /// <param name="socketInformation">The existing socket information.</param>
        public ScsTcpEndPoint(string ipAddress, int port, SocketInformation? socketInformation = null)
            : this(IPAddress.Parse(ipAddress), port, socketInformation)
        { }

        /// <summary>
        /// Creates a new ScsTcpEndPoint object with specified IP address and port number.
        /// </summary>
        /// <param name="ipAddress">IP address of the server</param>
        /// <param name="port">Listening TCP Port for incoming connection requests on server</param>
        /// <param name="socketInformation">The existing socket information.</param>
        public ScsTcpEndPoint(IPAddress ipAddress, int port, SocketInformation? socketInformation = null)
        {
            IpAddress = ipAddress;
            TcpPort = port;
            _existingSocketInformation = socketInformation;
        }
        
        /// <summary>
        /// Creates a new ScsTcpEndPoint from a string address.
        /// Address format must be like IPAddress:Port (For example: 127.0.0.1:10085).
        /// </summary>
        /// <param name="address">TCP end point Address</param>
        /// <returns>Created ScsTcpEndpoint object</returns>
        public ScsTcpEndPoint(string address)
        {
            var splittedAddress = address.Trim().Split(':');
            IpAddress = IPAddress.Parse(splittedAddress[0].Trim());
            TcpPort = Convert.ToInt32(splittedAddress[1].Trim());
        }

        /// <summary>
        /// Creates a Scs Server that uses this end point to listen incoming connections.
        /// </summary>
        /// <returns>Scs Server</returns>
        internal override IScsServer CreateServer()
        {
            return new ScsTcpServer(this);
        }

        /// <summary>
        /// Creates a Scs Client that uses this end point to connect to server.
        /// </summary>
        /// <returns>Scs Client</returns>
        internal override IScsClient CreateClient()
        {
            var client = new ScsTcpClient(this, _existingSocketInformation);
            _existingSocketInformation = null;
            return client;
        }

        /// <summary>
        /// Generates a string representation of this end point object.
        /// </summary>
        /// <returns>String representation of this end point object</returns>
        public override string ToString()
        {
            return IpAddress == null ? ("tcp://" + TcpPort) : ("tcp://" + IpAddress + ":" + TcpPort);
        }
    }
}