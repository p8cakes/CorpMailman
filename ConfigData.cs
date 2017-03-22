/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * ConfigData: Singleton class to manage all configuration data.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    #endregion

    #region ConfigData class
    /// <summary>
    /// Singleton class to manage all configuration data
    /// </summary>
    internal class ConfigData {

        #region Members
        /// <summary>Reference to ConfigData's instance - not lazy, constructed at AppDomain class-load</summary>
        private static ConfigData configData = new ConfigData();

        /// <summary>ThreadLock reference for making certain we can semaphore tasks</summary>
        private ThreadLock threadLock;

        /// <summary>Configured TestData for this run</summary>
        private TestDataStrings testData;
        #endregion

        #region Constructor
        /// <summary>
        /// Private constructor, since this is a singleton class
        /// </summary>
        private ConfigData() {
            this.LogLevel = LogLevels.Invalid;

            // Construct ThreadLock instance - default to true to make first caller run!
            this.threadLock = new ThreadLock(true);

            // Construct TestDataStrings instance
            this.testData = new TestDataStrings();

            // Set default configData duration to 900000d = 900 seconds, 15 minutes
            this.Duration = 900000d;

            // Parse TimerDuration from config, if defined
            ulong confDuration;
            var timerDuration = ConfigurationManager.AppSettings[Constants.Configuration.TimerDuration];

            if (ulong.TryParse(timerDuration, out confDuration)) {
                this.Duration = confDuration * Constants.Thousand;
            } else {
                throw new ConfigurationErrorsException(string.Format(Constants.Messages.MissingOrInvalidTimerDuration, timerDuration ?? Constants.Null));
            }

            // Parse logVerbose from config, if defined
            LogLevels logLevel;
            var logVerbose = ConfigurationManager.AppSettings[Constants.Configuration.LogLevel];

            if (!string.IsNullOrEmpty(logVerbose) && (Enum.TryParse(logVerbose, out logLevel))) {
                this.LogLevel = logLevel;
            }

            // Fail if no LogLevel specified in config
            if (this.LogLevel == LogLevels.Invalid) {
                throw new ConfigurationErrorsException(string.Format(Constants.Messages.MissingLogLevel, logVerbose ?? Constants.Empty));
            }

            // Get run once setting from config
            bool runOnce;
            if (bool.TryParse(ConfigurationManager.AppSettings[Constants.Configuration.RunOnce], out runOnce)) {
                this.RunOnce = runOnce;
            }

            // Get force interval setting from config
            bool forceInterval;
            if (bool.TryParse(ConfigurationManager.AppSettings[Constants.Configuration.ForceInterval], out forceInterval)) {
                this.ForceInterval = forceInterval;
            } else {
                // Default should be true
                this.ForceInterval = true;
            }

            // Get debug-test run one-time setting from config, default is false
            bool debugTestRunOneTime;
            if (bool.TryParse(ConfigurationManager.AppSettings[Constants.Configuration.DebugTestRunOneTime], out debugTestRunOneTime)) {
                this.DebugTestRunOneTime = debugTestRunOneTime;
            }

            // Get the StartAt value from config - the service would fail if this value occurs in the past for the current day
            var startAtParameter = ConfigurationManager.AppSettings[Constants.Configuration.StartAt];

            // Only proceed if the value is provided, or is not set to "[Now]" - both seem to be valid cases and we don't explicitly enforce a StartAt = "Now" consideration for configuration
            if (!string.IsNullOrEmpty(startAtParameter)) {

                // Check if this value is not equal to "[Now]"
                if (!startAtParameter.Equals(Constants.StartNow, StringComparison.InvariantCultureIgnoreCase)) {

                    DateTime startTimeValue;

                    var now = DateTime.UtcNow;
                    var cultureInfo = CultureInfo.InvariantCulture;

                    // Try and parse the date time value from configuration as a full date-time value
                    if (DateTime.TryParseExact(startAtParameter, Constants.FullDateTimeFormat, cultureInfo, DateTimeStyles.None, out startTimeValue)) {

                        // Set the startAt parameter to specified time
                        this.StartAt = startTimeValue;
                    } else if (DateTime.TryParseExact(startAtParameter, Constants.TimeFormat, cultureInfo, DateTimeStyles.None, out startTimeValue)) {

                        this.StartAt = startTimeValue;

                        // Add another day if this date-time occurs in the past
                        if (now > startTimeValue) {
                            this.StartAt = startTimeValue.AddDays(1);
                        }
                    } else {
                        // Fail as you couldn't parse the datetime as specified in configuration!
                        throw new ConfigurationErrorsException(string.Format(Constants.Messages.StartTimeIncorrect, startAtParameter));
                    }
                }
            }

            bool useEws;

            if (bool.TryParse(ConfigurationManager.AppSettings[Constants.Configuration.UseEws], out useEws)) {
                this.UseEws = useEws;
            }

            this.EwsUrl = ConfigurationManager.AppSettings[Constants.Configuration.EwsUrl];

            if (!this.UseEws) {
                this.SmtpServer = ConfigurationManager.AppSettings[Constants.Configuration.SmtpServer];

                if (string.IsNullOrEmpty(this.SmtpServer)) {
                    throw new ConfigurationErrorsException(Constants.Messages.MissingSmtpServerMessage);
                }
            }

            // MySQL Connection string part!
            // Get domain from config
            var mySqlEndpoint = ConfigurationManager.AppSettings[Constants.Configuration.Database.Endpoint];
            var mySqlDatabase = ConfigurationManager.AppSettings[Constants.Configuration.Database.DB];
            var mySqlLogin = ConfigurationManager.AppSettings[Constants.Configuration.Database.Login];
            var mySqlPassword = ConfigurationManager.AppSettings[Constants.Configuration.Database.Password];

            var builder = null as StringBuilder;

            foreach (var parameter in new string[] {
                        mySqlEndpoint,
                        mySqlDatabase,
                        mySqlLogin,
                        mySqlPassword
                    }) {
                // Fail if no parameter is found for this key
                if (string.IsNullOrEmpty(parameter)) {
                    // Construct StringBuilder for the first time
                    if (builder == null) {
                        builder = new StringBuilder();
                    }

                    if (builder.Length > 0) {
                        builder.Append(Constants.CommaSpace);
                    }

                    builder.Append(parameter);
                }
            }

            if (builder != null) {

                var builderMessage = builder.ToString();
                builder.Remove(0, builder.Length);

                throw new ConfigurationErrorsException(string.Format(Constants.Messages.Database.ConfigurationParameterMissing, builderMessage));
            }

            this.ConnectionString = string.Format(Constants.DatabaseConnectionString, mySqlEndpoint, mySqlDatabase, mySqlLogin, mySqlPassword);

            // Mail Credentials
            this.MailCredentials = ConfigurationManager.AppSettings[Constants.Configuration.Email.MailCredentials];

            if (string.IsNullOrEmpty(this.MailCredentials)) {
                // Fail as you can't work without SendAsName
                throw new ConfigurationErrorsException(Constants.Messages.MailCredentialsMissing);
            }

            // Mail Credentials, this can be null if we choose to employ the current user
            this.SendAsName = ConfigurationManager.AppSettings[Constants.Configuration.Email.SendAsName];

            // Login and password for a service account
            this.SamAccountName = ConfigurationManager.AppSettings[Constants.Configuration.Email.SamAccountName];
            this.SamAccountPassword = ConfigurationManager.AppSettings[Constants.Configuration.Email.SamAccountPassword];

            // TestData - read only if you want to first time test run
            if (this.DebugTestRunOneTime) {

                this.testData.From = ConfigurationManager.AppSettings[Constants.Configuration.TestData.From];
                this.testData.To = ConfigurationManager.AppSettings[Constants.Configuration.TestData.To];
                this.testData.Cc = ConfigurationManager.AppSettings[Constants.Configuration.TestData.Cc];
                this.testData.Bcc = ConfigurationManager.AppSettings[Constants.Configuration.TestData.Bcc];
                this.testData.SubjectPrefix = ConfigurationManager.AppSettings[Constants.Configuration.TestData.SubjectPrefix];
                this.testData.Subject = ConfigurationManager.AppSettings[Constants.Configuration.TestData.Subject];
                this.testData.Body = ConfigurationManager.AppSettings[Constants.Configuration.TestData.Body];

                // Get hasAttachments run one-time setting from config, default is false
                bool testDataHasAttachments;
                if (bool.TryParse(ConfigurationManager.AppSettings[Constants.Configuration.TestData.HasAttachments], out testDataHasAttachments)) {
                    this.testData.HasAttachments = testDataHasAttachments;
                }

                // Only if you have attachments
                if (this.testData.HasAttachments) {

                    if (ConfigurationManager.AppSettings.AllKeys.Contains(Constants.Configuration.TestData.Filename)) {
                        this.testData.Filename = ConfigurationManager.AppSettings[Constants.Configuration.TestData.Filename];
                    }

                    if (string.IsNullOrEmpty(this.TestData.Filename)) {
                        throw new ConfigurationErrorsException(Constants.Messages.MissingFilenameSettingMessage);
                    }

                    var counter = 0u;

                    builder = new StringBuilder();

                    var nextKey = null as string;

                    do {
                        nextKey = string.Format("{0}{1}", Constants.Configuration.TestData.Buffer, counter);

                        if (ConfigurationManager.AppSettings.AllKeys.Contains(nextKey)) {
                            builder.Append(ConfigurationManager.AppSettings[nextKey]);
                            counter++;
                        }
                    } while (ConfigurationManager.AppSettings.AllKeys.Contains(nextKey));

                    if (counter > 0) {
                        this.TestData.Attachment = builder.ToString();
                        builder.Remove(0, builder.Length);
                    } else {
                        throw new ConfigurationErrorsException(Constants.Messages.MissingAttachmentDataMessage);
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>Singleton instance accessor</summary>
        internal static ConfigData Instance { get { return ConfigData.configData; } }

        /// <summary>Whether this app is being run in debug mode</summary>
        internal bool Debug { get; set; }

        /// <summary>Duration we need to wait between subsequent audits</summary>
        internal Double Duration { get; private set; }

        /// <summary>StartAt value to specify when to run (UTC), null value means nothing has been set via config</summary>
        internal DateTime? StartAt { get; private set; }

        /// <summary>Level of logging desired</summary>
        internal LogLevels LogLevel { get; private set; }

        /// <summary>Whether this application should run just one time</summary>
        internal bool RunOnce { get; private set; }

        /// <summary>Whether this application should force the wait interval after finishing one iteration, or not</summary>
        internal bool ForceInterval { get; private set; }

        /// <summary>Whether we need to force/debug a test mail to be sent out when the system comes up</summary>
        internal bool DebugTestRunOneTime { get; private set; }

        /// <summary>Mail Credentials</summary>
        internal string MailCredentials { get; private set; }

        /// <summary>Mail Credentials (send mail on-behalf-of)</summary>
        internal string SendAsName { get; private set; }

        /// <summary>Mail Credentials (samAccountName to log in to Exchange Server)</summary>
        internal string SamAccountName { get; private set; }

        /// <summary>Mail Credentials (password to identify this service to Exchange Server)</summary>
        internal string SamAccountPassword { get; private set; }

        /// <summary>SMTP Server</summary>
        internal string SmtpServer { get; private set; }

        /// <summary>EWS URL - where the web service ASMX is defined</summary>
        internal string EwsUrl { get; private set; }

        /// <summary>Use Exchange Web Services to send email</summary>
        internal bool UseEws { get; private set; }

        /// <summary>Connection String to MySQL DB</summary>
        internal string ConnectionString { get; private set; }

        /// <summary>Gets the ThreadLock instance used to semaphore tasks</summary>
        internal ThreadLock ThreadLock { get { return this.threadLock; } }

        /// <summary>First Execution is Complete</summary>
        internal bool FirstExecutionComplete { get; set; }

        /// <summary>EncryptionKey - what we use to encrypt and send form data over to AWS</summary>
        internal byte[] EncryptionKey { get; private set; }

        /// <summary>API Key to call the Web Service</summary>
        internal string ApiKey { get; private set; }

        /// <summary>Gets a reference to the TestData instance for this configData</summary>
        internal TestDataStrings TestData { get { return this.testData; } }
        #endregion

        #region Methods
        #region public/internal Methods
        /// <summary>
        /// Convert a hexadecimal string to byte array
        /// </summary>
        /// <param name="hex">String in hexadecimal notation</param>
        /// <returns>Byte array</returns>
        internal static byte[] StringToByteArray(string hex) {

            var numberOfChars = hex.Length;
            var bytes = new byte[numberOfChars / 2];

            for (var i = 0; i < numberOfChars; i += 2) {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Convert byte array to hexadecimal string representation
        /// </summary>
        /// <param name="byteArray">Array of bytes</param>
        /// <returns>Hexadecimal string reprsentation</returns>
        internal static string ByteArrayToString(byte[] byteArray) {

            var hexBuilder = new StringBuilder(byteArray.Length * 2);

            foreach (var byteValue in byteArray) {
                hexBuilder.AppendFormat("{0:x2}", byteValue);
            }

            var hexString = hexBuilder.ToString();
            hexString.Remove(0, hexString.Length);

            return hexString;
        }

        /// <summary>
        /// Take the incoming text, employ the encryption key and initializationVector to encrypt this text and return its byte array back
        /// </summary>
        /// <param name="plainText">Plain text, to be encrypted</param>
        /// <param name="key">Encryption Key</param>
        /// <param name="initializationVector">Initialization vector</param>
        /// <returns>Byte Array with encrypted text</returns>
        internal static byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] initializationVector) {

            if (string.IsNullOrEmpty(plainText)) {
                throw new ArgumentNullException("plainText");
            }

            if ((key == null) || (key.Length == 0)) {
                throw new ArgumentNullException("Key");
            }

            if ((initializationVector == null) || (initializationVector.Length == 0)) {
                throw new ArgumentNullException("Initialization Vector");
            }

            var encrypted = null as byte[];

            // Create an RijndaelManaged object with the specified key and initalization vector
            using (var rijndaelObject = new RijndaelManaged()) {
                rijndaelObject.Key = key;
                rijndaelObject.IV = initializationVector;
                rijndaelObject.Mode = CipherMode.CBC;
                rijndaelObject.Padding = PaddingMode.Zeros;

                // Create a decrytor to perform the stream transform.
                var encryptor = rijndaelObject.CreateEncryptor(rijndaelObject.Key, rijndaelObject.IV);

                // Create the streams used for encryption.
                using (var encryptedMemoryStream = new MemoryStream()) {
                    using (var cryptoStream = new CryptoStream(encryptedMemoryStream, encryptor, CryptoStreamMode.Write)) {

                        using (var streamWriter = new StreamWriter(cryptoStream)) {

                            // Write all data to stream
                            streamWriter.Write(plainText);
                        }

                        encrypted = encryptedMemoryStream.ToArray();
                    }
                }
            }

            return encrypted;
        }
        #endregion
        #endregion

        #region TestData nested class
    internal class TestDataStrings {

            #region Properties
            /// <summary>Gets a reference to the from string in test data</summary>
            internal string From { get; set; }

            /// <summary>Gets a reference to the To string contained in test data</summary>
            internal string To { get; set; }

            /// <summary>Gets a reference to the Cc string contained in test data</summary>
            internal string Cc { get; set; }

            /// <summary>Gets a reference to the Bcc string contained in test data</summary>
            internal string Bcc { get; set; }

            /// <summary>Gets a reference to subject prefix for test data</summary>
            internal string SubjectPrefix { get; set; }

            /// <summary>Gets a reference to subject part of test data</summary>
            internal string Subject { get; set; }

            /// <summary>Gets a reference to body part of test data</summary>
            internal string Body { get; set; }

            /// <summary>Gets a reference to whether we have attachments in test data</summary>
            internal bool HasAttachments { get; set; }

            /// <summary>Gets a reference to test data filename</summary>
            internal string Filename { get; set; }

            /// <summary>Gets the attachment content as a Base-64 string</summary>
            internal string Attachment { get; set; }
            #endregion
        }
        #endregion
    }
    #endregion
}
