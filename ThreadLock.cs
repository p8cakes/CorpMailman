/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * ThreadLock class provides semaphore-like behavior.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    using System.Text;
    using System.Threading;
    using System.Collections.Generic;
    #endregion

    /// <summary>
    /// This class provides semaphore-like behavior that lets threads enqueue, waiting for a certain event.
    /// That event is the notification from an external thread to wake the first one (or everyone) up
    /// from the waiting list for a green light. All methods are thread-safe. Class maintained public, so
    /// client assemblies could leverage stop-n-go functionality themselves, outside of a thread pool by
    /// instantiating ThreadLock objects within their execution paths. Consult class documentation in code
    /// though. Employ this with one of the three following use cases:
    /// Use Case 1: Waiter & Caller alternate - WaitForNotification. Caller & Waiter - NotifyThread
    /// Use Case 2: Waiter (one of many that can be woken together) - WaitForMultiNotification. Caller - NotifyAllThreads
    /// Use Case 3: Waiter (self-sleeper) - FlagAndWaitForNotification. Caller - NotifyThread
    /// </summary>
    internal class ThreadLock {

        #region Constructors - 2 defined
        /// <summary>
        /// Construct a default ThreadLock instance, with default flag behavior set
        /// to true, so the first caller obtains authority to run
        /// </summary>
        internal ThreadLock() : this(true) { }

        /// <summary>
        /// Construct a ThreadLock instance with the provided initial flag behavior
        /// </summary>
        /// <param name="flag">If true, the first thread gets a green signal. If false, every thread
        /// must obtain the notification to run from another running thread</param>
        internal ThreadLock(bool flag) {
            this.ThreadFlag = flag;
        }
        #endregion

        #region ThreadLock Properties
        /// <summary>Gets (or privately sets) the maintained 'Flag' boolean value (true - green light, false - red)</summary>
        internal bool ThreadFlag { get; private set; }
        #endregion

        #region ThreadLock Methods
        #region public/internal Methods
        /// <summary>
        /// Wait for notification from an external thread, if it has been flagged off.  If flagged on,
        /// obtain threadLock monitor from this object and continue execution - only this thread runs
        /// </summary>
        internal void WaitForNotification() {
            // Obtain thread-safe lock for this block
            lock (this) {
                while (!this.ThreadFlag) {
                    // Check the ThreadFlag state to enter monitor
                    Monitor.Wait(this);
                }

                // Flag the thread back to false, so the waker-upper has to sleep too, if it comes here later
                this.ThreadFlag = false;
            }
        }

        /// <summary>
        /// Manually set flag to false, and proceed to wait for external notification
        /// </summary>
        internal void FlagAndWaitForNotification() {
            // Obtain thread-safe lock for this block
            lock (this) {
                this.ThreadFlag = false;      // Forcefully flag ThreadFlag to false, so we would stop at next

                while (!this.ThreadFlag) {
                    // Check for ThreadFlag state to enter monitor
                    Monitor.Wait(this);
                }

                // Flag the thread back to false, so the waker-upper has to sleep too, if it comes here later
                this.ThreadFlag = false;
            }
        }

        /// <summary>
        /// Wait for notification from an external thread, if ThreadFlag is set. If the wake up call
        /// is for many threads, they will all be woken up if they wait on this multi-notification
        /// </summary>
        internal void WaitForMultiNotification() {
            // Obtain thread-safe lock for this block
            lock (this) {
                while (!this.ThreadFlag) {
                    // Check the ThreadFlag state to enter monitor
                    Monitor.Wait(this);
                }
            }
        }

        /// <summary>
        /// Notify the first waiting thread to run
        /// </summary>
        internal void NotifyThread() {
            // Obtain thread-safe lock for this block
            lock (this) {
                this.ThreadFlag = true;       // Set the ThreadFlag to true, so we clear the semaphore
                Monitor.Pulse(this);           // Pulse the first thread waiting on this monitor

                // Coerce the current thread to yield - so it doesn't force-grab ThreadFlag in the same processor iteration
                Thread.Sleep(0);
            }
        }

        /// <summary>
        /// Notify all waiting threads to run
        /// </summary>
        internal void NotifyAllThreads() {
            // Obtain thread-safe lock for this block
            lock (this) {
                this.ThreadFlag = true;       // Set the ThreadFlag to true, so we clear the semaphore
                Monitor.PulseAll(this);        // Pulse all threads waiting on this monitor

                // Coerce the current thread to yield - so it doesn't force-grab ThreadFlag in the same processor iteration
                Thread.Sleep(0);
            }
        }
        #endregion
        #endregion
    };
}
