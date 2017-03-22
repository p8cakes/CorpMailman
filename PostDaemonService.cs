/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * CorpMailmanService service class that implements ServiceBase methods to perform tasks on start and stop.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.ServiceProcess;
    using System.Text;
    using System.Timers;
    #endregion

    #region CorpMailmanService class
    /// <summary>
    /// CorpMailmanService service class that implements ServiceBase methods to perform tasks on start and stop.
    /// </summary>
    internal partial class CorpMailmanService : ServiceBase {

        #region Members
        /// <summary>Will be set with service shutdown initiated</summary>
        private bool shutdown = false;

        /// <summary>Timer instance to manage connections</summary>
        private Timer connectTimer;

        /// <summary>Reference to ConfigData's singleton instance</summary>
        private ConfigData configData;
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor.
        /// </summary>
        internal CorpMailmanService() {

            InitializeComponent();

            this.configData = ConfigData.Instance;
        }
        #endregion

        #region Methods
        #region public/internal Methods
        #region ServiceBase implementation
        /// <summary>
        /// OnStart Implementation.
        /// </summary>
        /// <param name="args">Arguments provided to program being started.</param>
        protected override void OnStart(string[] args) {

            // Call Start
            this.Start(args, debugFlag: false);
        }

        /// <summary>
        /// OnStop implementation.
        /// </summary>
        protected override void OnStop() {

            // Set shutdown to true to stop any tasks from being fired via timer
            this.shutdown = true;

            // Log stoppage attempt on threadpool threads if set to VeryVerbose
            if (this.configData.LogLevel == LogLevels.VeryVerbose) {
                EventLog.WriteEntry(Constants.System.NtServiceName, Constants.Messages.FlaggingThreadPoolStart, EventLogEntryType.Information, (int)EventCodes.FlagThreadPoolStoppage);
            }

            // Wait until any active worker thread from the Threadpool finishes with the assigned task below
            this.configData.ThreadLock.WaitForNotification();

            // Log stoppage if not set to Normal
            if (this.configData.LogLevel != LogLevels.Normal) {
                EventLog.WriteEntry(Constants.System.NtServiceName, Constants.Messages.StoppingService, EventLogEntryType.Information, (int)EventCodes.StoppingService);
            }
        }
        #endregion

        /// <summary>
        /// Start method that can be called from Program too.
        /// </summary>
        /// <param name="args">Command line args</param>
        /// <pparam name="debugFlag">Debug mode</pparam>
        internal void Start(string[] args, bool debugFlag) {

            if (this.configData.LogLevel != LogLevels.Normal) {
                // Log startup entry to Event log
                EventLog.WriteEntry(Constants.System.NtServiceName, Constants.Messages.StartingService, EventLogEntryType.Information, (int)EventCodes.StartingService);
            }

            // Whether this app is being run in debug mode
            this.configData.Debug = debugFlag;

            // For release mode, where Windows Service behavior is desired (not Command-line console app), hand off this task to Worker thread
            if (!debugFlag) {
                // Add this to ThreadPool
                System.Threading.ThreadPool.QueueUserWorkItem(
                    o => {
                        connectTimer_Elapsed(null, null);
                    }
                );
            } else {

                if (!this.configData.StartAt.HasValue) {
                    // Run manually via current thread
                    connectTimer_Elapsed(null, null);
                } else {

                    // Get the value set for our nullable StartAt member
                    var startTime = this.configData.StartAt.Value;

                    // Construct new Timer object, assign it the delegate it needs to run
                    this.connectTimer = new Timer();
                    this.connectTimer.Elapsed += new ElapsedEventHandler(connectTimer_Elapsed);

                    // Fail if you find that the startTime is in the past
                    if (DateTime.UtcNow.CompareTo(startTime) >= 0) {

                        // Get the string part of message to furnish to exception message
                        var startTimeAsString = startTime.ToString(Constants.FullDateTimeFormat);
                        throw new ConfigurationErrorsException(string.Format(Constants.Messages.StartTimeInPast, startTimeAsString));
                    } else {
                        if (this.configData.LogLevel != LogLevels.Normal) {

                            // Log when the startup time has been scheduled for
                            var scheduledFor = string.Format(Constants.Messages.ServiceFirstRunScheduledFor, startTime.ToString(Constants.FullDateTimeFormat));

                            // Log startup entry to Event log
                            EventLog.WriteEntry(Constants.System.NtServiceName, scheduledFor, EventLogEntryType.Information, (int)EventCodes.StartingService);
                        }
                    }

                    // Now find how many milliseconds from now until startTime - set connectTimer Interval to this value
                    var milliseconds = this.configData.StartAt.Value.Subtract(DateTime.UtcNow).TotalMilliseconds;

                    connectTimer.Interval = milliseconds;

                    // Restart after completing your task
                    connectTimer.Start();
                }
            }
        }

        #region Event Handlers
        /// <summary>
        /// Event fired when timer duration elapsed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs instance</param>
        private void connectTimer_Elapsed(object sender, ElapsedEventArgs e) {

            // Return if we are shutting down the Windows Sevice
            if (this.shutdown) {
                return;
            }

            // Timestamp for service start
            DateTime startTime = DateTime.UtcNow;

            // Stop the timer - this run may take a long time
            if (this.connectTimer != null) {
                this.connectTimer.Stop();
            }

            // Emit logging
            if (this.configData.LogLevel == LogLevels.VeryVerbose) {
                EventLog.WriteEntry(Constants.System.NtServiceName, Constants.Messages.LaunchingNewTask, EventLogEntryType.Information, (int)EventCodes.StartingTask);
            }

            #region Meat of the matter - actual tasks to accomplish
            try {
                // Daemon: make it do its work
                var daemon = new Daemon();

                // Run test one time if configured to do so!
                if (!this.configData.FirstExecutionComplete) {
                    this.configData.FirstExecutionComplete = true;

                    // Dispatch test email
                    if (this.configData.DebugTestRunOneTime) {
                        daemon.CreateNewTestEmails();
                    }
                }

                // Main task
                daemon.FetchEmailsToSendAndDispatchThemAll();
            } catch (CorpMailmanException ex) {
                // Log CorpMailmanException stack trace - don't propagate outwards
                EventLog.WriteEntry(Constants.System.NtServiceName, ex.ToString(), EventLogEntryType.Error, (int)EventCodes.Error);
            }
            #endregion

            // If we need to run just once, then don't add this task back
            if (!this.configData.RunOnce) {
                // Create connectTimer for the first call
                if (this.connectTimer == null) {
                    this.connectTimer = new Timer();
                    this.connectTimer.Elapsed += new ElapsedEventHandler(connectTimer_Elapsed);
                }

                // Now find how many milliseconds elapsed from epoch above
                double milliseconds = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;

                // If the total time is greater than this duration, obtain difference to set interval
                if ((this.configData.Duration > milliseconds) && (!this.configData.ForceInterval)) {
                    connectTimer.Interval = (this.configData.Duration - milliseconds);
                } else {
                    // Reset back to original 
                    connectTimer.Interval = this.configData.Duration;
                }

                // Restart after completing your task
                connectTimer.Start();

                // Log completion for extreme verbose
                if (this.configData.LogLevel == LogLevels.VeryVerbose) {
                    EventLog.WriteEntry(Constants.System.NtServiceName, Constants.Messages.CompletedNewTask, EventLogEntryType.Information, (int)EventCodes.CompletedTask);
                }
            }
        }
        #endregion
        #endregion
        #endregion
    }
    #endregion
}