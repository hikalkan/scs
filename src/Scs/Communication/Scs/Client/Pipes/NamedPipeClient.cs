using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Pipes;
using Hik.Communication.Scs.Communication.EndPoints.Pipes;

namespace Hik.Communication.Scs.Client.Pipes
{
    /// <summary>
    /// Named pipe implementation of <see cref="IScsClient"/>.
    /// </summary>
    internal class NamedPipeClient : ScsClientBase
    {
        private readonly NamedPipeEndPoint _endPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeClient"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        public NamedPipeClient(NamedPipeEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        #region Overrides of ScsClientBase

        /// <inheritdoc />
        protected override ICommunicationChannel CreateCommunicationChannel()
        {
            return new NamedPipeCommunicationChannel(_endPoint);
        }

        #endregion
    }
}
