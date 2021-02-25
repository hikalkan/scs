namespace Hik.Communication.SslScs.Authentication
{
    /// <summary>
    /// This provides the SSL authentication mechanisms
    /// </summary>
    public enum SslScsAuthMode
    {
        /// <summary>
        /// The ServerAuthenticationOnly allows the client to verify the server using its public key
        /// </summary>
        ServerAuth,
        /// <summary>
        /// Both entities verify each other
        /// </summary>
        MutualAuth,
    }
}
