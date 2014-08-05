﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Hik.Threading
{
    /// <summary>
    /// This class is used to process items sequentially in a multithreaded manner.
    /// </summary>
    /// <typeparam name="TItem">Type of item to process</typeparam>
    public class SequentialItemProcessor<TItem>
    {
        #region Private fields

        /// <summary>
        /// The method delegate that is called to actually process items.
        /// </summary>
        private readonly Action<TItem> _processMethod;

        /// <summary>
        /// Item queue. Used to process items sequentially.
        /// </summary>
        private readonly Queue<TItem> _queue;

        /// <summary>
        /// Indicates state of the item processing.
        /// </summary>
        private bool _isProcessing;

        /// <summary>
        /// A boolean value to control running of SequentialItemProcessor.
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// An object to synchronize threads.
        /// </summary>
        private readonly object _syncObj = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new SequentialItemProcessor object.
        /// </summary>
        /// <param name="processMethod">The method delegate that is called to actually process items</param>
        public SequentialItemProcessor(Action<TItem> processMethod)
        {
            _processMethod = processMethod;
            _queue = new Queue<TItem>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds an item to queue to process the item.
        /// </summary>
        /// <param name="item">Item to add to the queue</param>
        public void EnqueueMessage(TItem item)
        {
            //Add the item to the queue and start a new Task if needed
            lock (_syncObj)
            {
                if (!_isRunning)
                {
                    return;
                }

                _queue.Enqueue(item);

                if (!_isProcessing)
                {
                    ThreadPool.QueueUserWorkItem(ProcessItem);
                }
            }
        }

        /// <summary>
        /// Starts processing of items.
        /// </summary>
        public void Start()
        {
            _isRunning = true;
        }

        /// <summary>
        /// Stops processing of items and waits stopping of current item.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;

            //Clear all incoming messages
            lock (_syncObj) 
            {
                _queue.Clear();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// This method runs on a new separated Task (thread) to process
        /// items on the queue.
        /// </summary>
        private void ProcessItem(object state)
        {
            //Try to get an item from queue to process it.
            TItem itemToProcess;
            lock (_syncObj)
            {
                if (!_isRunning || _isProcessing)
                {
                    return;
                }

                if (_queue.Count <= 0)
                {
                    return;
                }

                _isProcessing = true;
                itemToProcess = _queue.Dequeue();
            }

            try
            {
                //Process the item (by calling the _processMethod delegate)
                _processMethod(itemToProcess);
            }
            catch { }

            //Process next item if available
            lock (_syncObj)
            {
                _isProcessing = false;
                if (!_isRunning || _queue.Count <= 0)
                {
                    return;
                }

                //Start a new task
                ThreadPool.QueueUserWorkItem(ProcessItem);
            }
        }

        #endregion
    }
}
