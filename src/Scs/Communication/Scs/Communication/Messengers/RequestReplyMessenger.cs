using System;
using System.Collections.Generic;
using System.Threading;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.Scs.Communication.Protocols;
using Hik.Threading;

namespace Hik.Communication.Scs.Communication.Messengers
{
    /// <summary>
    /// This class adds SendMessageAndWaitForResponse(...) and SendAndReceiveMessage methods
    /// to a IMessenger for synchronous request/response style messaging.
    /// It also adds queued processing of incoming messages.
    /// </summary>
    /// <typeparam name="T">Type of IMessenger object to use as underlying communication</typeparam>
    public class RequestReplyMessenger<T> : IMessenger, IDisposable where T : IMessenger
    {
        #region Public events

        /// <summary>
        /// This event is raised when a new message is received from underlying messenger.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        /// <summary>
        /// This event is raised when a new message is sent without any error.
        /// It does not guaranties that message is properly handled and processed by remote application.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageSent;

        #endregion

        #region Public properties

        /// <summary>
        /// Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        public IScsWireProtocol WireProtocol
        {
            get { return Messenger.WireProtocol; }
            set { Messenger.WireProtocol = value; }
        }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastReceivedMessageTime
        {
            get
            {
                return Messenger.LastReceivedMessageTime;
            }
        }

        /// <summary>
        /// Gets the time of the last succesfully received message.
        /// </summary>
        public DateTime LastSentMessageTime
        {
            get
            {
                return Messenger.LastSentMessageTime;
            }
        }

        /// <summary>
        /// Gets the underlying IMessenger object.
        /// </summary>
        public T Messenger { get; private set; }

        /// <summary>
        /// Timeout value as milliseconds to wait for a receiving message on 
        /// SendMessageAndWaitForResponse and SendAndReceiveMessage methods.
        /// Default value: 60000 (1 minute).
        /// </summary>
        public int Timeout { get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// Default Timeout value.
        /// </summary>
        private const int DefaultTimeout = 60000;

        /// <summary>
        /// This messages are waiting for a response those are used when 
        /// SendMessageAndWaitForResponse is called.
        /// Key: MessageID of waiting request message.
        /// Value: A WaitingMessage instance.
        /// </summary>
        private readonly SortedList<string, WaitingMessage> _waitingMessages;

        /// <summary>
        /// This object is used to process incoming messages sequentially.
        /// </summary>
        private readonly SequentialItemProcessor<IScsMessage> _incomingMessageProcessor;

        /// <summary>
        /// This object is used for thread synchronization.
        /// </summary>
        private readonly object _syncObj = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new RequestReplyMessenger.
        /// </summary>
        /// <param name="messenger">IMessenger object to use as underlying communication</param>
        public RequestReplyMessenger(T messenger)
        {
            Messenger = messenger;
            messenger.MessageReceived += Messenger_MessageReceived;
            messenger.MessageSent += Messenger_MessageSent;
            _incomingMessageProcessor = new SequentialItemProcessor<IScsMessage>(OnMessageReceived);
            _waitingMessages = new SortedList<string, WaitingMessage>();
            Timeout = DefaultTimeout;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Starts the messenger.
        /// </summary>
        public virtual void Start()
        {
            _incomingMessageProcessor.Start();
        }

        /// <summary>
        /// Stops the messenger.
        /// Cancels all waiting threads in SendMessageAndWaitForResponse method and stops message queue.
        /// SendMessageAndWaitForResponse method throws exception if there is a thread that is waiting for response message.
        /// Also stops incoming message processing and deletes all messages in incoming message queue.
        /// </summary>
        public virtual void Stop()
        {
            _incomingMessageProcessor.Stop();

            //Pulse waiting threads for incoming messages, since underlying messenger is disconnected
            //and can not receive messages anymore.
            lock (_syncObj)
            {
                foreach (var waitingMessage in _waitingMessages.Values)
                {
                    waitingMessage.State = WaitingMessageStates.Cancelled;
                    waitingMessage.WaitEvent.Set();
                }

                _waitingMessages.Clear();
            }
        }

        /// <summary>
        /// Calls Stop method of this object.
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">Message to be sent</param>
        public void SendMessage(IScsMessage message)
        {
            Messenger.SendMessage(message);
        }

        /// <summary>
        /// Sends a message and waits a response for that message.
        /// </summary>
        /// <remarks>
        /// Response message is matched with RepliedMessageId property, so if
        /// any other message (that is not reply for sent message) is received
        /// from remote application, it is not considered as a reply and is not
        /// returned as return value of this method.
        /// 
        /// MessageReceived event is not raised for response messages.
        /// </remarks>
        /// <param name="message">message to send</param>
        /// <returns>Response message</returns>
        public IScsMessage SendMessageAndWaitForResponse(IScsMessage message)
        {
            return SendMessageAndWaitForResponse(message, Timeout);
        }

        /// <summary>
        /// Sends a message and waits a response for that message.
        /// </summary>
        /// <remarks>
        /// Response message is matched with RepliedMessageId property, so if
        /// any other message (that is not reply for sent message) is received
        /// from remote application, it is not considered as a reply and is not
        /// returned as return value of this method.
        /// 
        /// MessageReceived event is not raised for response messages.
        /// </remarks>
        /// <param name="message">message to send</param>
        /// <param name="timeoutMilliseconds">Timeout duration as milliseconds.</param>
        /// <returns>Response message</returns>
        /// <exception cref="TimeoutException">Throws TimeoutException if can not receive reply message in timeout value</exception>
        /// <exception cref="CommunicationException">Throws CommunicationException if communication fails before reply message.</exception>
        public IScsMessage SendMessageAndWaitForResponse(IScsMessage message, int timeoutMilliseconds)
        {
            //Create a waiting message record and add to list
            var waitingMessage = new WaitingMessage();
            lock (_syncObj)
            {
                _waitingMessages[message.MessageId] = waitingMessage;
            }

            try
            {
                //Send message
                Messenger.SendMessage(message);

                //Wait for response
                waitingMessage.WaitEvent.Wait(timeoutMilliseconds);

                //Check for exceptions
                switch (waitingMessage.State)
                {
                    case WaitingMessageStates.WaitingForResponse:
                        throw new TimeoutException("Timeout occured. Can not received response.");
                    case WaitingMessageStates.Cancelled:
                        throw new CommunicationException("Disconnected before response received.");
                }

                //return response message
                return waitingMessage.ResponseMessage;
            }
            finally
            {
                //Remove message from waiting messages
                lock (_syncObj)
                {
                    if (_waitingMessages.ContainsKey(message.MessageId))
                    {
                        _waitingMessages.Remove(message.MessageId);
                    }
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles MessageReceived event of Messenger object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void Messenger_MessageReceived(object sender, MessageEventArgs e)
        {
            //Check if there is a waiting thread for this message in SendMessageAndWaitForResponse method
            if (!string.IsNullOrEmpty(e.Message.RepliedMessageId))
            {
                WaitingMessage waitingMessage = null;
                lock (_syncObj)
                {
                    if (_waitingMessages.ContainsKey(e.Message.RepliedMessageId))
                    {
                        waitingMessage = _waitingMessages[e.Message.RepliedMessageId];
                    }
                }

                //If there is a thread waiting for this response message, pulse it
                if (waitingMessage != null)
                {
                    waitingMessage.ResponseMessage = e.Message;
                    waitingMessage.State = WaitingMessageStates.ResponseReceived;
                    waitingMessage.WaitEvent.Set();
                    return;
                }
            }

            _incomingMessageProcessor.EnqueueMessage(e.Message);
        }

        /// <summary>
        /// Handles MessageSent event of Messenger object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void Messenger_MessageSent(object sender, MessageEventArgs e)
        {
            OnMessageSent(e.Message);
        }

        #endregion

        #region Event raising methods

        /// <summary>
        /// Raises MessageReceived event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageReceived(IScsMessage message)
        {
            var handler = MessageReceived;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(message));
            }
        }

        /// <summary>
        /// Raises MessageSent event.
        /// </summary>
        /// <param name="message">Received message</param>
        protected virtual void OnMessageSent(IScsMessage message)
        {
            var handler = MessageSent;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(message));
            }
        }

        #endregion

        #region WaitingMessage class

        /// <summary>
        /// This class is used to store messaging context for a request message
        /// until response is received.
        /// </summary>
        private sealed class WaitingMessage
        {
            /// <summary>
            /// Response message for request message 
            /// (null if response is not received yet).
            /// </summary>
            public IScsMessage ResponseMessage { get; set; }

            /// <summary>
            /// ManualResetEvent to block thread until response is received.
            /// </summary>
            public ManualResetEventSlim WaitEvent { get; private set; }

            /// <summary>
            /// State of the request message.
            /// </summary>
            public WaitingMessageStates State { get; set; }

            /// <summary>
            /// Creates a new WaitingMessage object.
            /// </summary>
            public WaitingMessage()
            {
                WaitEvent = new ManualResetEventSlim(false);
                State = WaitingMessageStates.WaitingForResponse;
            }
        }

        /// <summary>
        /// This enum is used to store the state of a waiting message.
        /// </summary>
        private enum WaitingMessageStates
        {
            /// <summary>
            /// Still waiting for response.
            /// </summary>
            WaitingForResponse,

            /// <summary>
            /// Message sending is cancelled.
            /// </summary>
            Cancelled,

            /// <summary>
            /// Response is properly received.
            /// </summary>
            ResponseReceived
        }

        #endregion
    }
}
