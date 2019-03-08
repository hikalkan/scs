using Hik.Communication.Scs.Communication.EndPoints;

namespace Hik.Communication.Scs.Server
{
    /// <summary>
    /// This class is used to create SCS servers.
    /// </summary>
    public static class ScsServerFactory
    {
        /// <summary>
        /// Creates a new SCS Server using an EndPoint.
        /// </summary>
        /// <param name="endPoint">Endpoint that represents address of the server</param>
        /// <returns>Created TCP server</returns>
        public static IScsServer CreateServer(ScsEndPoint endPoint)
        {
            return endPoint.CreateServer();
        }
    }
}
