﻿#region (c)2009-2011 Lokad - New BSD license
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence
#endregion

using System;
using System.Threading;

namespace Lokad.Cloud.Storage.Shared.Threading
{
    // HACK: imported back from Lokad.Shared

    /// <summary>
    /// Helper class for invoking tasks with timeout. Overhead is 0,005 ms.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public sealed class WaitFor<TResult>
    {
        readonly TimeSpan _timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitFor{T}"/> class, 
        /// using the specified timeout for all operations.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public WaitFor(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        /// <summary>
        /// Executes the specified function within the current thread, aborting it
        /// if it does not complete within the specified timeout interval. 
        /// </summary>
        /// <param name="function">The function.</param>
        /// <returns>result of the function</returns>
        /// <remarks>
        /// The performance trick is that we do not interrupt the current
        /// running thread. Instead, we just create a watcher that will sleep
        /// until the originating thread terminates or until the timeout is
        /// elapsed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if function is null</exception>
        /// <exception cref="TimeoutException">if the function does not finish in time </exception>
        public TResult Run(Func<TResult> function)
        {
            if (function == null) throw new ArgumentNullException("function");

            var sync = new object();
            var isCompleted = false;

            WaitCallback watcher = obj =>
            {
                var watchedThread = obj as Thread;

                lock (sync)
                {
                    if (!isCompleted)
                    {
                        Monitor.Wait(sync, _timeout);
                    }
                }

                // CAUTION: the call to Abort() can be blocking in rare situations
                // http://msdn.microsoft.com/en-us/library/ty8d3wta.aspx
                // Hence, it should not be called with the 'lock' as it could deadlock
                // with the 'finally' block below.

                if (!isCompleted)
                {
                    watchedThread.Abort();
                }
            };

            try
            {
                ThreadPool.QueueUserWorkItem(watcher, Thread.CurrentThread);
                return function();
            }
            catch (ThreadAbortException)
            {
                // This is our own exception.
                Thread.ResetAbort();

                throw new TimeoutException(string.Format("The operation has timed out after {0}.", _timeout));
            }
            finally
            {
                lock (sync)
                {
                    isCompleted = true;
                    Monitor.Pulse(sync);
                }
            }
        }

        /// <summary>
        /// Executes the specified function within the current thread, aborting it
        /// if it does not complete within the specified timeout interval.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="function">The function.</param>
        /// <returns>result of the function</returns>
        /// <remarks>
        /// The performance trick is that we do not interrupt the current
        /// running thread. Instead, we just create a watcher that will sleep
        /// until the originating thread terminates or until the timeout is
        /// elapsed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">if function is null</exception>
        /// <exception cref="TimeoutException">if the function does not finish in time </exception>
        public static TResult Run(TimeSpan timeout, Func<TResult> function)
        {
            return new WaitFor<TResult>(timeout).Run(function);
        }
    }
}
