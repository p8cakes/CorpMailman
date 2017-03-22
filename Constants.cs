/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * Constants static class to collect and list all strings used in the Windows Service.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Constants class
    /// <summary>
    /// Constants static class to collect and list all strings used in the Windows Service
    /// </summary>
    internal static class Constants {

        #region Members
        /// <summary>One thousand, to convert seconds to milliseconds</summary>
        internal const double Thousand = 1000d;

        /// <summary>Null, as string</summary>
        internal const string Null = "null";

        /// <summary>Yes, as string</summary>
        internal const string Yes = "Yes";

        /// <summary>No, as string</summary>
        internal const string No = "No";

        /// <summary>The comma</summary>
        internal const char Comma = ',';

        /// <summary>Open Square bracket string</summary>
        internal const string OpenSquareBracket = "[";

        /// <summary>Close Square bracket with space string</summary>
        internal const string CloseSquareBracketSpace = "] ";

        /// <summary>Comma array</summary>
        internal static readonly char[] CommaArray = new char[] {
                                                         Constants.Comma
                                                     };

        /// <summary>The comma with a trailing space</summary>
        internal const string CommaSpace = ", ";

        /// <summary>The semicolon</summary>
        internal const char Semicolon = ';';

        /// <summary>Command-line option to run this in debug mode</summary>
        internal const string DebugOption = "-debug";

        /// <summary>Empty message for data not-found</summary>
        internal const string Empty = "[Empty]";

        /// <summary>Start now - value for the StartAt parameter below to do this right away!</summary>
        internal const string StartNow = "[Now]";

        /// <summary>Key attribute in XML</summary>
        internal const string Key = "key";

        /// <summary>Value attribute in XML</summary>
        internal const string Value = "value";

        /// <summary>Database Connection String</summary>
        internal const string DatabaseConnectionString = "SERVER={0};DATABASE={1};UID={2};PASSWORD={3}";

        /// <summary>Time Format string used for parsing</summary>
        internal const string FullDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>Time Format string used for parsing</summary>
        internal const string TimeFormat = "HH:mm:ss";

        /// <summary>From field in the email</summary>
        internal const string FromField = "<b>From:</b>&nbsp;";

        /// <summary>Sent field in the email</summary>
        internal const string SentField = "<br/><b>Sent:</b>&nbsp;";

        /// <summary>UTC followed by Horizontal Rule, to be suffixed to the metadata header in the email</summary>
        internal const string UtcHrField = "&nbsp;UTC<hr/>";

        /// <summary>HTTPS, defined as a string</summary>
        internal const string Https = "https";

        /// <summary>Default port for SMTP (25)</summary>
        internal const int DefaultSmtpPort = 25;

        #region System class
        /// <summary>
        /// Collect together all System-related constants
        /// </summary>
        internal static class System {

            #region Members
            /// <summary>Service name key.</summary>
            internal const string ServiceKey = "CorpMailman";

            /// <summary>App Name to register on NT Service task list.</summary>
            internal const string NtServiceName = "CorpMailman";

            /// <summary>Service Description on NT Service task list.</summary>
            internal const string ServiceDesc = "Service to scan database for mails that need to be sent, and dispatch them";

            /// <summary>Application Event type</summary>
            internal const string Application = "Application";
            #endregion
        }
        #endregion

        #region Messages class
        /// <summary>
        /// Collect together all Message strings
        /// </summary>
        internal static class Messages {

            #region Members
            /// <summary>Starting Service message</summary>
            internal const string StartingService = "Starting Service";

            /// <summary>Stopping Service message</summary>
            internal const string StoppingService = "Stopping Service";

            /// <summary>Event log status message to be written on task start-up.</summary>
            internal const string LaunchingNewTask = "Launching new task";

            /// <summary>Event log status message to be written on task completion.</summary>
            internal const string CompletedNewTask = "Completed new task";

            // <summary>Event log status message to be written when Stop is clicked and a worker may be busy.</summary>
            internal const string FlaggingThreadPoolStart = "Attempting to shut down service, waiting for a busy worker";

            /// <summary>Status message for Post Daemon object being called to perform tasks</summary>
            internal const string CorpMailmanSingletonInService = "Post Daemon instance in service, performing tasks";

            /// <summary>Service first run has been scheduled for</summary>
            internal const string ServiceFirstRunScheduledFor = "Service first run scheduled for: {0}";

            /// <summary>Found an email to dispatch</summary>
            internal const string FoundEmailToDispatch = "{0} UTC:Found a mail to send to {1}";

            /// <summary>Found an email to dispatch</summary>
            internal const string MailDispatchStarted = "Sending email to {0}";

            /// <summary>Mail has been dispatched</summary>
            internal const string MailDispatched = "{0} UTC:Mail sent!";

            /// <summary>No To: address found for this record, deleting it</summary>
            internal const string NoToAddressFound = "No To: Address found for email with mailId:{0}, deleting that.";

            /// <summary>Exception message to throw when LogLevel string is not supplied to set incorrectly</summary>
            internal const string MissingLogLevel = "Invalid or missing LogLevels key in config file, please use 1, 2, or 3. Found: {0}";

            /// <summary>Exception message to throw when ForceInterval string is not supplied to set incorrectly</summary>
            internal const string MissingForceInterval = "Invalid or missing ForceInterval key in config file, please use true or false. Found: {0}";

            /// <summary>Exception message to throw when configured StartAt value occurs in the past - it has to be in the future</summary>
            internal const string StartTimeInPast = "Start time occurs in the past. Found: {0}, please employ one in the future!";

            /// <summary>Exception message to throw when configured StartAt value is incorrect, and can't be parsed!</summary>
            internal const string StartTimeIncorrect = "Start time is incorrect: {0}, please employ one that is correct!";

            /// <summary>Exception message to throw when TimerDuration key in the configuration has an invalid value</summary>
            internal const string MissingOrInvalidTimerDuration = "Missing or invalid TimerDuration value in configuration! Found: {0}";

            /// <summary>Exception message to throw when MailCredentials name parameter was not found in the configuration</summary>
            internal const string MailCredentialsMissing = "MailCredentials key not found! Please employ a correct value";

            /// <summary>Exception message to throw when you can't convert Active Directory Date</summary>
            internal const string ErrorConvertingDate = "Error converting the date value for: {0}";

            /// <summary>Exception message to throw when no filename has been defined for attachment, when configured so</summary>
            internal const string MissingFilenameSettingMessage = "TestData.Filename key has not been defined in App.config";

            /// <summary>Exception message to throw when attachment data is missing, when we need to send out a test message with attachments</summary>
            internal const string MissingAttachmentDataMessage = "TestData.Buffer0 and further nodes are missing, for Base64 encoded attachment data";

            /// <summary>Exception message to throw when AWS API URL is missing, and user has specified that way to propagate mail</summary>
            internal const string MissingAwsUrlMessage = "Please furnish the URL for the AWS Mail API endpoint if you seek to send mail that way.";

            /// <summary>Exception message to throw when API Key is missing, and user has specified that way to propagate mail</summary>
            internal const string MissingApiKeyMessage = "Please furnish the API Key to call the AWS Mail API if you seek to send mail that way.";

            /// <summary>Exception message to throw when SmtpServer Key is missing, and user has specified that way to propagate mail</summary>
            internal const string MissingSmtpServerMessage = "Missing SMTP Server, please furnish correct value.";
            #endregion

            #region Database class
            /// <summary>
            /// Collect together all Database messages
            /// </summary>
            internal static class Database {
                #region Members
                /// <summary>Exception message to throw when one or more configuration parameters missing for database</summary>
                internal const string ConfigurationParameterMissing = "Configuration Parameter(s) for database absent: {0}";

                /// <summary>Exception message to throw when you cannot connect to the database</summary>
                internal const string CannotConnect = "Cannot connect to the database";

                /// <summary>Exception message to throw when invalid login/password combination is supplied</summary>
                internal const string InvalidCredentials = "Invalid login/password supplied";
                #endregion
            }
            #endregion
        }
        #endregion

        #region Table class
        /// <summary>
        /// Collect together all Database table-related constants for column names and stored procedures
        /// </summary>
        internal static class Table {

            #region Mails class
            /// <summary>
            /// Column names for the Mails table we execute and fetch data
            /// </summary>
            internal static class Mails {
                #region Members
                /// <summary>Mail ID column in mails table</summary>
                internal const string MailId = "mailId";

                /// <summary>Sender column in mails table</summary>
                internal const string Sender = "sender";

                /// <summary>Recipients column in mails table</summary>
                internal const string Recipients = "recipients";

                /// <summary>ccRecipients column in mails table</summary>
                internal const string CcRecipients = "ccRecipients";

                /// <summary>bccRecipients column in mails table</summary>
                internal const string BccRecipients = "bccRecipients";

                /// <summary>Subject column in mails table</summary>
                internal const string Subject = "subject";

                /// <summary>Subject Prefix column in mails table</summary>
                internal const string SubjectPrefix = "subjectPrefix";

                /// <summary>Body column in mails table</summary>
                internal const string Body = "Body";

                /// <summary>Has Attachments column in mails table</summary>
                internal const string HasAttachments = "hasAttachments";

                /// <summary>Created column in mails table</summary>
                internal const string Created = "created";

                /// <summary>Direct column in mails table</summary>
                internal const string Direct = "direct";

                /// <summary>Importance column in mails table</summary>
                internal const string Importance = "importance";
                #endregion
            }
            #endregion

            #region All the Attachments we need to send
            /// <summary>
            /// All the attachments that we need to dispatch with a certain email
            /// </summary>
            internal static class Attachments {
                #region Members
                /// <summary>Filename column of the attachments table</summary>
                public const string Filename = "filename";

                /// <summary>
                /// Filesize column of the attachments table
                /// </summary>
                public const string Filesize = "filesize";

                /// <summary>Attachment column for binary data in the table</summary>
                public const string Attachment = "attachment";
                #endregion
            }
            #endregion

            #region Queries class
            /// <summary>
            /// Queries that we execute to fetch data and update those too
            /// </summary>
            internal static class Queries {
                #region Members
                /// <summary>Get Emails to send out from the app query</summary>
                internal const string GetEmailToSend = "call getEmailToSend();";

                /// <summary>Get Attachments (if any) for this email query</summary>
                internal const string GetAttachmentsForEmail = "call getAttachmentsForEmail(@mailId);";

                /// <summary>Add this email to the list of mails on server to be dispatched</summary>
                internal const string AddEmail = @"call addEmail(@sender,
                                                                     @recipients,
                                                                     @ccRecipients,
                                                                     @bccRecipients,
                                                                     @subject,
                                                                     @subjectPrefix,
                                                                     @body,
                                                                     @markMailAsReady,
                                                                     @hasAttachments,
                                                                     @importance,
                                                                     @direct,
                                                                     @timestamp
                                                                 );";

                /// <summary>Add this Mail attachment for a certain mail query</summary>
                internal const string AddMailAttachment = "call addMailAttachment(@mailId,@filename,@filesize,@attachment);";

                /// <summary>Mark email as ready query</summary>
                internal const string MarkEmailAsReady = "call markEmailAsReady(@mailId);";

                /// <summary>Delete this email from the database query</summary>
                internal const string DeleteEmail = "call deleteEmail(@mailId);";

                #region Parameters we furnish to stored procedures
                /// <summary>
                /// Parameters we furnish to stored procedures
                /// </summary>
                internal static class Parameters {
                    #region Members
                    /// <summary>Mail ID parameter</summary>
                    internal const string MailId = "@mailId";

                    /// <summary>Sender parameter</summary>
                    internal const string Sender = "@sender";

                    /// <summary>Recipients parameter</summary>
                    internal const string Recipients = "@recipients";

                    /// <summary>CcRecipients parameter</summary>
                    internal const string CcRecipients = "@ccRecipients";

                    /// <summary>BccRecipients parameter</summary>
                    internal const string BccRecipients = "@bccRecipients";

                    /// <summary>Importance parameter</summary>
                    internal const string Importance = "@importance";

                    /// <summary>Subject parameter</summary>
                    internal const string Subject = "@subject";

                    /// <summary>Subject Prefix parameter</summary>
                    internal const string SubjectPrefix = "@subjectPrefix";

                    /// <summary>Body</summary>
                    internal const string Body = "@body";

                    /// <summary>Has Attachments parameter</summary>
                    internal const string HasAttachments = "@hasAttachments";

                    /// <summary>Mark this email as ready to send parameter, used when no attachments exist</summary>
                    internal const string MarkMailAsReady = "@markMailAsReady";

                    /// <summary>File name</summary>
                    internal const string Filename = "@filename";

                    /// <summary>File size parameter</summary>
                    internal const string Filesize = "@filesize";

                    /// <summary>Attachment binary or text data, as bytes</summary>
                    internal const string Attachment = "@attachment";

                    /// <summary>Whether email has to be composed directly - no top header</summary>
                    internal const string Direct = "@direct";

                    /// <summary>Timestamp parameter, when is this email ready to be sent?</summary>
                    internal const string Timestamp = "@timestamp";
                    #endregion
                }
                #endregion
                #endregion
            }
            #endregion
        }
        #endregion

        #region Configuration class
        /// <summary>
        /// Collect together all Configuration-related strings
        /// </summary>
        internal static class Configuration {

            #region Members
            /// <summary>Configuration key for timer duration.</summary>
            internal const string TimerDuration = "TimerDuration";

            /// <summary>Configuration key for event logging (1, 2, 3 or 4)</summary>
            internal const string LogLevel = "LogLevel";

            /// <summary>When do we start this, a blank value or [Now] means right now!</summary>
            internal const string StartAt = "StartAt";

            /// <summary>Whether this application needs to run just once</summary>
            internal const string RunOnce = "RunOnce";

            /// <summary>Whether this application needs to force the interval between runs?</summary>
            internal const string ForceInterval = "ForceInterval";

            /// <summary>Whether this application needs to force create an email and send it out once when the system comes up</summary>
            internal const string DebugTestRunOneTime = "DebugTestRunOneTime";

            /// <summary>Exchange Web Services URL</summary>
            internal const string EwsUrl = "EWSURL";

            /// <summary>Use EWS to dispatch email</summary>
            internal const string UseEws = "UseEWS";

            /// <summary>SMTP Server</summary>
            internal const string SmtpServer = "SMTPServer";

            #region Database class
            /// <summary>
            /// Collect together all configuration strings for database connections
            /// </summary>
            internal static class Database {

                #region Members
                /// <summary>Endpoint parameter</summary>
                internal const string Endpoint = "Endpoint";

                /// <summary>Database parameter</summary>
                internal const string DB = "Database";

                /// <summary>Login Parameter</summary>
                internal const string Login = "Login";

                /// <summary>Password parameter</summary>
                internal const string Password = "Password";
                #endregion
            }
            #endregion

            #region Email class
            /// <summary>
            /// Collect together all configuration strings for notification emails
            /// </summary>
            internal static class Email {

                #region Members
                /// <summary>Mail Credentials</summary>
                internal const string MailCredentials = "MailCredentials";

                /// <summary>From</summary>
                internal const string SendAsName = "SendAsName";

                /// <summary>samAccountName (login value)</summary>
                internal const string SamAccountName = "SamAccountName";

                /// <summary>samAccountName password</summary>
                internal const string SamAccountPassword = "SamAccountPassword";
                #endregion
            }
            #endregion

            #region TestData class
            /// <summary>
            /// Collect together all configuration strings for TestData to confirm everything works as expected one-time
            /// </summary>
            internal static class TestData {

                #region Members
                /// <summary>From parameter</summary>
                internal const string From = "TestData.From";

                /// <summary>To parameter</summary>
                internal const string To = "TestData.To";

                /// <summary>CC: parameter</summary>
                internal const string Cc = "TestData.Cc";

                /// <summary>BCC: parameter</summary>
                internal const string Bcc = "TestData.Bcc";

                /// <summary>SubjectPrefix Parameter</summary>
                internal const string SubjectPrefix = "TestData.SubjectPrefix";

                /// <summary>Subject parameter</summary>
                internal const string Subject = "TestData.Subject";

                /// <summary>Body parameter</summary>
                internal const string Body = "TestData.Body";

                /// <summary>HasAttachments parameter</summary>
                internal const string HasAttachments = "TestData.HasAttachments";

                /// <summary>Filename parameter</summary>
                internal const string Filename = "TestData.Filename";

                /// <summary>Buffer parameter</summary>
                internal const string Buffer = "TestData.Buffer";
                #endregion
            }
            #endregion
            #endregion
        }
        #endregion
        #endregion
    }
    #endregion
}
