using System.Reflection;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.ScsServices.Communication.Messages;
using Hik.Proxy;

namespace Hik.Communication.ScsServices.Communication
{
    /// <summary>
    /// This class is used to generate a dynamic proxy to invoke remote methods.
    /// It translates method invocations to messaging.
    /// </summary>
    /// <typeparam name="TProxy">Type of the proxy class/interface</typeparam>
    /// <typeparam name="TMessenger">Type of the messenger object that is used to send/receive messages</typeparam>
    internal class RemoteInvokeProxy<TProxy, TMessenger> : IInterceptor where TMessenger : IMessenger
    {
        /// <summary>
        /// Messenger object that is used to send/receive messages.
        /// </summary>
        private readonly RequestReplyMessenger<TMessenger> _clientMessenger;

        /// <summary>
        /// Creates a new RemoteInvokeProxy object.
        /// </summary>
        /// <param name="clientMessenger">Messenger object that is used to send/receive messages</param>
        public RemoteInvokeProxy(RequestReplyMessenger<TMessenger> clientMessenger)
        {
            _clientMessenger = clientMessenger;
        }

        /// <summary>
        /// 将方法调用转换为远程方法调用
        /// </summary>
        /// <param name="method">代理方法</param>
        /// <param name="parameters">代理参数</param>
        /// <returns>方法返回结果</returns>
        public virtual object Intercept(MethodInfo method, object[] parameters)
        {
            if (method == null)
            {
                return null;
            }

            var requestMessage = new ScsRemoteInvokeMessage
            {
                ServiceClassName = typeof(TProxy).Name,
                MethodName = method.Name,
                Parameters = parameters
            };

            var responseMessage = _clientMessenger.SendMessageAndWaitForResponse(requestMessage) as ScsRemoteInvokeReturnMessage;
            if (responseMessage == null)
            {
                return null;
            }

            return responseMessage.RemoteException == null
                ? responseMessage.ReturnValue
                : throw responseMessage.RemoteException;
        }

        ///// <summary>
        ///// Overrides message calls and translates them to messages to remote application.
        ///// </summary>
        ///// <param name="msg">Method invoke message (from RealProxy base class)</param>
        ///// <returns>Method invoke return message (to RealProxy base class)</returns>
        //public override IMessage Invoke(IMessage msg)
        //{
        //    var message = msg as IMethodCallMessage;
        //    if (message == null)
        //    {
        //        return null;
        //    }

        //    var requestMessage = new ScsRemoteInvokeMessage
        //    {
        //        ServiceClassName = typeof (TProxy).Name,
        //        MethodName = message.MethodName,
        //        Parameters = message.InArgs
        //    };

        //    var responseMessage = _clientMessenger.SendMessageAndWaitForResponse(requestMessage) as ScsRemoteInvokeReturnMessage;
        //    if (responseMessage == null)
        //    {
        //        return null;
        //    }

        //    return responseMessage.RemoteException != null
        //               ? new ReturnMessage(responseMessage.RemoteException, message)
        //               : new ReturnMessage(responseMessage.ReturnValue, null, 0, message.LogicalCallContext, message);
        //}
    }
}