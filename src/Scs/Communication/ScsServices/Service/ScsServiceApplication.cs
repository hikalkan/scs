using System;
using System.Collections.Generic;
using System.Reflection;
using Hik.Collections;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Messengers;
using Hik.Communication.Scs.Server;
using Hik.Communication.ScsServices.Communication.Messages;

namespace Hik.Communication.ScsServices.Service
{
    /// <summary>
    /// Implements IScsServiceApplication and provides all functionallity.
    /// </summary>
    internal class ScsServiceApplication : IScsServiceApplication
    {
        #region Public events

        /// <summary>
        /// This event is raised when a new client connected to the service.
        /// </summary>
        public event EventHandler<ServiceClientEventArgs> ClientConnected;
       
        /// <summary>
        /// This event is raised when a client disconnected from the service.
        /// </summary>
        public event EventHandler<ServiceClientEventArgs> ClientDisconnected;

        #endregion

        #region Private fields

        /// <summary>
        /// Underlying IScsServer object to accept and manage client connections.
        /// </summary>
        private readonly IScsServer _scsServer;

        /// <summary>
        /// User service objects that is used to invoke incoming method invocation requests.
        /// Key: Service interface type's name.
        /// Value: Service object.
        /// </summary>
        private readonly ThreadSafeSortedList<string, ServiceObject> _serviceObjects;

        /// <summary>
        /// All connected clients to service.
        /// Key: Client's unique Id.
        /// Value: Reference to the client.
        /// </summary>
        private readonly ThreadSafeSortedList<long, IScsServiceClient> _serviceClients;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new ScsServiceApplication object.
        /// </summary>
        /// <param name="scsServer">Underlying IScsServer object to accept and manage client connections</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if scsServer argument is null</exception>
        public ScsServiceApplication(IScsServer scsServer)
        {
            if (scsServer == null)
            {
                throw new ArgumentNullException("scsServer");
            }

            _scsServer = scsServer;
            _scsServer.ClientConnected += ScsServer_ClientConnected;
            _scsServer.ClientDisconnected += ScsServer_ClientDisconnected;
            _serviceObjects = new ThreadSafeSortedList<string, ServiceObject>();
            _serviceClients = new ThreadSafeSortedList<long, IScsServiceClient>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts service application.
        /// </summary>
        public void Start()
        {
            _scsServer.Start();
        }

        /// <summary>
        /// Stops service application.
        /// </summary>
        public void Stop()
        {
            _scsServer.Stop();
        }

        /// <summary>
        /// Adds a service object to this service application.
        /// Only single service object can be added for a service interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <typeparam name="TServiceClass">Service class type. Must be delivered from ScsService and must implement TServiceInterface.</typeparam>
        /// <param name="service">An instance of TServiceClass.</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if service argument is null</exception>
        /// <exception cref="Exception">Throws Exception if service is already added before</exception>
        public void AddService<TServiceInterface, TServiceClass>(TServiceClass service) 
            where TServiceClass : ScsService, TServiceInterface
            where TServiceInterface : class
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            var type = typeof(TServiceInterface);
            if(_serviceObjects[type.Name] != null)
            {
                throw new Exception("Service '" + type.Name + "' is already added before.");                
            }

            _serviceObjects[type.Name] = new ServiceObject(type, service);
        }

        /// <summary>
        /// Removes a previously added service object from this service application.
        /// It removes object according to interface type.
        /// </summary>
        /// <typeparam name="TServiceInterface">Service interface type</typeparam>
        /// <returns>True: removed. False: no service object with this interface</returns>
        public bool RemoveService<TServiceInterface>()
            where TServiceInterface : class
        {
            return _serviceObjects.Remove(typeof(TServiceInterface).Name);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles ClientConnected event of _scsServer object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void ScsServer_ClientConnected(object sender, ServerClientEventArgs e)
        {
            var requestReplyMessenger = new RequestReplyMessenger<IScsServerClient>(e.Client);
            requestReplyMessenger.MessageReceived += Client_MessageReceived;
            requestReplyMessenger.Start();

            var serviceClient = ScsServiceClientFactory.CreateServiceClient(e.Client, requestReplyMessenger);
            _serviceClients[serviceClient.ClientId] = serviceClient;
            OnClientConnected(serviceClient);
        }

        /// <summary>
        /// Handles ClientDisconnected event of _scsServer object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void ScsServer_ClientDisconnected(object sender, ServerClientEventArgs e)
        {
            var serviceClient = _serviceClients[e.Client.ClientId];
            if (serviceClient == null)
            {
                return;
            }

            _serviceClients.Remove(e.Client.ClientId);
            OnClientDisconnected(serviceClient);
        }

        /// <summary>
        /// Handles MessageReceived events of all clients, evaluates each message,
        /// finds appropriate service object and invokes appropriate method.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void Client_MessageReceived(object sender, MessageEventArgs e)
        {
            //Get RequestReplyMessenger object (sender of event) to get client
            var requestReplyMessenger = (RequestReplyMessenger<IScsServerClient>) sender;

            //Cast message to ScsRemoteInvokeMessage and check it
            var invokeMessage = e.Message as ScsRemoteInvokeMessage;
            if (invokeMessage == null)
            {
                return;
            }

            try
            {
                //Get client object
                var client = _serviceClients[requestReplyMessenger.Messenger.ClientId];
                if (client == null)
                {
                    requestReplyMessenger.Messenger.Disconnect();
                    return;
                }

                //Get service object
                var serviceObject = _serviceObjects[invokeMessage.ServiceClassName];
                if (serviceObject == null)
                {
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException("There is no service with name '" + invokeMessage.ServiceClassName + "'"));
                    return;
                }

                //Invoke method
                try
                {
                    object returnValue;
                    //Set client to service, so user service can get client
                    //in service method using CurrentClient property.
                    serviceObject.Service.CurrentClient = client;
                    try
                    {
                        returnValue = serviceObject.InvokeMethod(invokeMessage.MethodName, invokeMessage.Parameters);
                    }
                    finally
                    {
                        //Set CurrentClient as null since method call completed
                        serviceObject.Service.CurrentClient = null;
                    }

                    //Send method invocation return value to the client
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, returnValue, null);
                }
                catch (TargetInvocationException ex)
                {
                    var innerEx = ex.InnerException;
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException(innerEx.Message + Environment.NewLine + "Service Version: " + serviceObject.ServiceAttribute.Version, innerEx));
                    return;
                }
                catch (Exception ex)
                {
                    SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException(ex.Message + Environment.NewLine + "Service Version: " + serviceObject.ServiceAttribute.Version, ex));
                    return;
                }
            }
            catch (Exception ex)
            {
                SendInvokeResponse(requestReplyMessenger, invokeMessage, null, new ScsRemoteException("An error occured during remote service method call.", ex));
                return;
            }
        }

        /// <summary>
        /// Sends response to the remote application that invoked a service method.
        /// </summary>
        /// <param name="client">Client that sent invoke message</param>
        /// <param name="requestMessage">Request message</param>
        /// <param name="returnValue">Return value to send</param>
        /// <param name="exception">Exception to send</param>
        private static void SendInvokeResponse(IMessenger client, IScsMessage requestMessage, object returnValue, ScsRemoteException exception)
        {
            try
            {
                client.SendMessage(
                    new ScsRemoteInvokeReturnMessage
                        {
                            RepliedMessageId = requestMessage.MessageId,
                            ReturnValue = returnValue,
                            RemoteException = exception
                        });
            }
            catch
            {

            }
        }

        /// <summary>
        /// Raises ClientConnected event.
        /// </summary>
        /// <param name="client"></param>
        private void OnClientConnected(IScsServiceClient client)
        {
            var handler = ClientConnected;
            if (handler != null)
            {
                handler(this, new ServiceClientEventArgs(client));
            }
        }

        /// <summary>
        /// Raises ClientDisconnected event.
        /// </summary>
        /// <param name="client"></param>
        private void OnClientDisconnected(IScsServiceClient client)
        {
            var handler = ClientDisconnected;
            if (handler != null)
            {
                handler(this, new ServiceClientEventArgs(client));
            }
        }

        #endregion

        #region ServiceObject class

        /// <summary>
        /// Represents a user service object.
        /// It is used to invoke methods on a ScsService object.
        /// </summary>
        private sealed class ServiceObject
        {
            /// <summary>
            /// The service object that is used to invoke methods on.
            /// </summary>
            public ScsService Service { get; private set; }

            /// <summary>
            /// ScsService attribute of Service object's class.
            /// </summary>
            public ScsServiceAttribute ServiceAttribute { get; private set; }

            /// <summary>
            /// This collection stores a list of all methods of service object.
            /// Key: Method name
            /// Value: Informations about method. 
            /// </summary>
            private readonly SortedList<string, MethodInfo> _methods;

            /// <summary>
            /// Creates a new ServiceObject.
            /// </summary>
            /// <param name="serviceInterfaceType">Type of service interface</param>
            /// <param name="service">The service object that is used to invoke methods on</param>
            public ServiceObject(Type serviceInterfaceType, ScsService service)
            {
                Service = service;
                var classAttributes = serviceInterfaceType.GetCustomAttributes(typeof(ScsServiceAttribute), true);
                if (classAttributes.Length <= 0)
                {
                    throw new Exception("Service interface (" + serviceInterfaceType.Name + ") must has ScsService attribute.");
                }

                ServiceAttribute = classAttributes[0] as ScsServiceAttribute;
                _methods = new SortedList<string, MethodInfo>();
                foreach (var methodInfo in serviceInterfaceType.GetMethods())
                {
                    _methods.Add(methodInfo.Name, methodInfo);
                }
            }

            /// <summary>
            /// Invokes a method of Service object.
            /// </summary>
            /// <param name="methodName">Name of the method to invoke</param>
            /// <param name="parameters">Parameters of method</param>
            /// <returns>Return value of method</returns>
            public object InvokeMethod(string methodName, params object[] parameters)
            {
                //Check if there is a method with name methodName
                if (!_methods.ContainsKey(methodName))
                {
                    throw new Exception("There is not a method with name '" + methodName + "' in service class.");
                }

                //Get method
                var method = _methods[methodName];

                //Invoke method and return invoke result
                return method.Invoke(Service, parameters);
            }
        }

        #endregion
    }
}
