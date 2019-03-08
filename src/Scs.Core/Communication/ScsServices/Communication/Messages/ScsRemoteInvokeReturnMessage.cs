using System;
using Hik.Communication.Scs.Communication.Messages;

namespace Hik.Communication.ScsServices.Communication.Messages
{
    /// <summary>
    /// This message is sent as response message to a ScsRemoteInvokeMessage.
    /// It is used to send return value of method invocation.
    /// </summary>
    [Serializable]
    public class ScsRemoteInvokeReturnMessage : ScsMessage
    {
        /// <summary>
        /// Return value of remote method invocation.
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// If any exception occured during method invocation, this field contains Exception object.
        /// If no exception occured, this field is null.
        /// </summary>
        public ScsRemoteException RemoteException { get; set; }

        /// <summary>
        /// Represents this object as string.
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString()
        {
            return string.Format("ScsRemoteInvokeReturnMessage: Returns {0}, Exception = {1}", ReturnValue, RemoteException);
        }
    }
}
