using Hik.Communication.Scs.Communication.Channels;
using Hik.Communication.Scs.Communication.Channels.Pipes;
using Hik.Communication.Scs.Communication.EndPoints.Pipes;

namespace Hik.Communication.Scs.Server.Pipes
{
    /// <summary>
    /// Named pipe implementation of <see cref="IScsServer"/>.
    /// </summary>
    internal class NamedPipeServer : ScsServerBase
    {
        private readonly NamedPipeEndPoint _endPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedPipeServer"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public NamedPipeServer(NamedPipeEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        #region Overrides of ScsServerBase

        /// <inheritdoc />
        protected override IConnectionListener CreateConnectionListener()
        {
            return new NamedPipeConnectionListener(_endPoint);
        }

        #endregion
    }
}
