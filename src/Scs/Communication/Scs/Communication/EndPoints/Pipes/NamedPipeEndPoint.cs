using System;
using System.Threading;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Client.Pipes;
using Hik.Communication.Scs.Server;
using Hik.Communication.Scs.Server.Pipes;

namespace Hik.Communication.Scs.Communication.EndPoints.Pipes
{
    /// <summary>
    /// Named pipe implementation of <see cref="ScsEndPoint"/>.
    /// </summary>
    public sealed class NamedPipeEndPoint : ScsEndPoint
    {
        #region Constants

        internal const string PROTOCOL = "pipe";
        internal const int DEFAULT_CONNECTION_TIMEOUT_SECONDS = 15;

        #endregion

        /// <summary>
        /// Gets the endpoint address.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the connection timeout in seconds or <see cref="Timeout.Infinite"/>.
        /// </summary>
        public int ConnectionTimeout { get; set; }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeEndPoint" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="System.ArgumentException">Value is null or empty.;address</exception>
        public NamedPipeEndPoint(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value is null or empty.", "name");

            Name = name;
            ConnectionTimeout = DEFAULT_CONNECTION_TIMEOUT_SECONDS;
        }

        #endregion

        #region Overrides of ScsEndPoint

        /// <inheritdoc />
        internal override IScsServer CreateServer()
        {
            return new NamedPipeServer(this);
        }

        /// <inheritdoc />
        internal override IScsClient CreateClient()
        {
            return new NamedPipeClient(this);
        }

        #endregion
    }
}
