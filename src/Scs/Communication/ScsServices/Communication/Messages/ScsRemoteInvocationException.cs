using System;
using System.Runtime.Serialization;

namespace Hik.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// Exception thrown when service invocation target errors.
    /// </summary>
    [Serializable]
    public class ScsRemoteInvocationException : ScsRemoteException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScsRemoteInvocationException"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="serviceVersion">The service version.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ScsRemoteInvocationException(string serviceType, string serviceVersion, string methodName, string message, Exception innerException)
            : base(message, innerException)
        {
            MethodName = methodName;
            ServiceType = serviceType;
            ServiceVersion = serviceVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScsRemoteInvocationException"/> class.
        /// </summary>
        /// <param name="serializationInfo"></param>
        /// <param name="context"></param>
        private ScsRemoteInvocationException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
            MethodName = serializationInfo.GetString("MethodName");
            ServiceType = serializationInfo.GetString("ServiceType");
            ServiceVersion = serializationInfo.GetString("ServiceVersion");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the name of the invoked method.
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// Gets the type of the service class.
        /// </summary>
        public string ServiceType { get; private set; }

        /// <summary>
        /// Gets the service version.
        /// </summary>
        public string ServiceVersion { get; private set; }

        #endregion

        #region Overrides of Exception

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("MethodName", MethodName);
            info.AddValue("ServiceType", ServiceType);
            info.AddValue("ServiceVersion", ServiceVersion);
        }

        #endregion
    }
}
