/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * Daemon class to perform checking for mails, and dispatch them via Exchange Web Services
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    using Microsoft.Exchange.WebServices.Data;
    #endregion

    #region Daemon class
    /// <summary>
    /// Daemon class to perform checking for mails, and dispatch them via Exchange Web Services
    /// </summary>
    internal class Daemon {

        #region Members
        /// <summary>Reference to ConfigData singleton instance</summary>
        private ConfigData configData;

        /// <summary>DBConnect instance</summary>
        private DBConnect dbConnect;
        #endregion

        #region Constructor
        /// <summary>
        /// Obtain references to ConfigData singleton, construct DBConnect instance
        /// </summary>
        internal Daemon() {

            // Get reference to ConfigData singleton
            this.configData = ConfigData.Instance;

            // New DBConnect instance for talking to DB
            this.dbConnect = new DBConnect();
        }
        #endregion

        #region Methods
        #region public/internal Methods
        /// <summary>
        /// Main method - well, do something!
        /// </summary>
        internal void FetchEmailsToSendAndDispatchThemAll() {

            if (this.configData.LogLevel == LogLevels.VeryVerbose) {
                // Log startup entry to Event log
                System.Diagnostics.EventLog.WriteEntry(Constants.System.NtServiceName, Constants.Messages.StartingService, System.Diagnostics.EventLogEntryType.Information, (int)EventCodes.StartingService);
            }

            var nextMail = null as Mail;

            // Do for every email you fetch from the DB
            while ((nextMail = dbConnect.GetNextMailToSend()) != null) {

                // Proceed only if you have a valid To: address
                if (!string.IsNullOrEmpty(nextMail.ToAddresses)) {

                    // Display mail dispatch operation
                    if (this.configData.Debug) {
                        Console.WriteLine(string.Format(Constants.Messages.FoundEmailToDispatch, DateTime.UtcNow.ToLongTimeString(), nextMail.ToAddresses));
                    }

                    if (this.configData.LogLevel == LogLevels.VeryVerbose) {
                        // Log email address to Event log
                        System.Diagnostics.EventLog.WriteEntry(Constants.System.NtServiceName,
                            string.Format(Constants.Messages.MailDispatchStarted,
                                            nextMail.ToAddresses),
                            System.Diagnostics.EventLogEntryType.Information,
                            (int)EventCodes.StartingTask);
                    }

                    // AWS Workflow - encrypt the JSON text and dispatch it on to the URL over HTTP Post
                    if (this.configData.UseEws) {

                        // Current Exchange installation is Exchange 2013 SP2 
                        var service = new ExchangeService(ExchangeVersion.Exchange2010_SP2);


                        if ((!string.IsNullOrEmpty(this.configData.SamAccountName)) &&
                            (!string.IsNullOrEmpty(this.configData.SamAccountPassword))) {
                             service.Credentials = new WebCredentials(this.configData.SamAccountName, this.configData.SamAccountPassword);
                        }

                        if (!string.IsNullOrEmpty(this.configData.EwsUrl)) {
                            service.Url = new Uri(this.configData.EwsUrl);
                        } else {
                            service.AutodiscoverUrl(this.configData.MailCredentials, Daemon.RedirectionCallback);
                        }

                        var message = new EmailMessage(service);

                        // Subject ought to be [KeyForThisApp] Whatever foo you want 
                        var builder = new StringBuilder();

                        // Send direct mails with no artificial header above body of text 
                        if (nextMail.Direct) {
                             message.Subject = nextMail.Subject;
                         } else {

                             if (!string.IsNullOrEmpty(nextMail.SubjectPrefix)) {
                                 // Subject ought to be [KeyForThisApp] Whatever foo you want 
                                 builder.Append(Constants.OpenSquareBracket);
                                 builder.Append(nextMail.SubjectPrefix);
                                 builder.Append(Constants.CloseSquareBracketSpace);
                                 builder.Append(nextMail.Subject);

                                 message.Subject = builder.ToString();
                                 builder.Remove(0, builder.Length);
                             } else {
                                 message.Subject = nextMail.Subject;
                             }

                             // Construct metadata header for actual sender 
                             builder.Append(Constants.FromField);
                             builder.Append(nextMail.From);
                             builder.Append(Constants.SentField);
                             builder.Append(nextMail.Created.GetValueOrDefault());
                             builder.Append(Constants.UtcHrField);
                         }

                         if (!string.IsNullOrEmpty(this.configData.SendAsName)) {
                             message.From = this.configData.SendAsName;
                         } else {
                             message.From = this.configData.MailCredentials;
                         }

                         // Add actual body 
                         builder.Append(nextMail.Body);
                         message.Body = builder.ToString();
                         builder.Remove(0, builder.Length);

                         // Add each recipient 
                         foreach (var recipient in nextMail.ToAddressList) {
                             message.ToRecipients.Add(recipient);
                         }

                         // Add cc: recipients, if any 
                         if (nextMail.CcAddressList != null) {
                             foreach (var recipient in nextMail.CcAddressList) {
                                 message.CcRecipients.Add(recipient);
                             }
                         }

                         // Add Bcc: recipients, if any 
                         if (nextMail.BccAddressList != null) {
                             foreach (var recipient in nextMail.BccAddressList) {
                                 message.BccRecipients.Add(recipient);
                             }
                         }

                         // Set mail as important if desired so 
                         if (nextMail.Importance) {
                             message.Importance = Importance.High;
                         }

                         // Add each attachment 
                         if (nextMail.HasAttachments.GetValueOrDefault()) {
                             foreach (var attachment in nextMail.Attachments) {
                                 // Construct new Attachment object and add it to our message 
                                 message.Attachments.AddFileAttachment(attachment.Filename, attachment.Bytes);
                             }
                         }


                         // Send the message! 
                         message.Send();

                    } else {
                        // Current Exchange installation is Exchange 2013 SP2
                        var smtpClient = new SmtpClient(this.configData.SmtpServer);

                        if ((!string.IsNullOrEmpty(this.configData.SamAccountName)) &&
                            (!string.IsNullOrEmpty(this.configData.SamAccountPassword))) {
                            smtpClient.Credentials = new NetworkCredential(this.configData.SamAccountName, this.configData.SamAccountPassword);
                        }

                        smtpClient.Port = Constants.DefaultSmtpPort;

                        var message = new MailMessage();
                        message.IsBodyHtml = true;

                        // Subject ought to be [KeyForThisApp] Whatever foo you want
                        var builder = new StringBuilder();

                        // Send direct mails with no artificial header above body of text
                        if (nextMail.Direct) {
                            message.Subject = nextMail.Subject;
                        } else {

                            if (!string.IsNullOrEmpty(nextMail.SubjectPrefix)) {
                                // Subject ought to be [KeyForThisApp] Whatever foo you want
                                builder.Append(Constants.OpenSquareBracket);
                                builder.Append(nextMail.SubjectPrefix);
                                builder.Append(Constants.CloseSquareBracketSpace);
                                builder.Append(nextMail.Subject);

                                message.Subject = builder.ToString();
                                builder.Remove(0, builder.Length);
                            } else {
                                message.Subject = nextMail.Subject;
                            }

                            // Construct metadata header for actual sender
                            builder.Append(Constants.FromField);
                            builder.Append(nextMail.From);
                            builder.Append(Constants.SentField);
                            builder.Append(nextMail.Created.GetValueOrDefault());
                            builder.Append(Constants.UtcHrField);
                        }

                        if (!string.IsNullOrEmpty(this.configData.SendAsName)) {
                            message.From = new MailAddress(this.configData.SendAsName);
                        } else {
                            message.From = new MailAddress(this.configData.MailCredentials);
                        }

                        // Add actual body
                        builder.Append(nextMail.Body);
                        message.Body = builder.ToString();
                        builder.Remove(0, builder.Length);

                        // Add each recipient
                        foreach (var recipient in nextMail.ToAddressList) {
                            message.To.Add(recipient);
                        }

                        // Add cc: recipients, if any
                        if (nextMail.CcAddressList != null) {
                            foreach (var recipient in nextMail.CcAddressList) {
                                message.CC.Add(recipient);
                            }
                        }

                        // Add Bcc: recipients, if any
                        if (nextMail.BccAddressList != null) {
                            foreach (var recipient in nextMail.BccAddressList) {
                                message.Bcc.Add(recipient);
                            }
                        }

                        // Set mail as important if desired so
                        if (nextMail.Importance) {
                            message.Priority = MailPriority.High;
                        }

                        // Add each attachment
                        if (nextMail.HasAttachments.GetValueOrDefault()) {
                            foreach (var attachment in nextMail.Attachments) {

                                var memoryStream = new MemoryStream(attachment.Bytes);

                                // Construct new Attachment object and add it to our message
                                var mailAttachment = new System.Net.Mail.Attachment(memoryStream, attachment.Filename);
                                message.Attachments.Add(mailAttachment);
                            }
                        }

                        // Send the message!
                        smtpClient.Send(message);
                    }

                    // Display mail dispatch operation
                    if (this.configData.Debug) {
                        Console.WriteLine(string.Format(Constants.Messages.MailDispatched, DateTime.UtcNow.ToLongTimeString()));
                    }
                } else {
                    // Log email address to Event log
                    System.Diagnostics.EventLog.WriteEntry(Constants.System.NtServiceName,
                        string.Format(Constants.Messages.NoToAddressFound,
                                        nextMail.MailId),
                        System.Diagnostics.EventLogEntryType.Warning,
                        (int)EventCodes.EmptyEmail);
                }

                // Delete this email from DB
                this.dbConnect.DeleteEmail(nextMail.MailId.GetValueOrDefault());
            }
        }

        /// <summary>
        /// Callback to employ when looking up auto-forward URLs
        /// </summary>
        /// <param name="redirectionUrl">URL to check</param>
        /// <returns></returns>
        private static bool RedirectionCallback(string redirectionUrl) {

            // The default for the validation callback is to reject the URL.
            var result = false;

            var redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == Constants.Https) {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Create and save a new email with an image attachment, just to test the mail system out one time!
        /// </summary>
        internal void CreateNewTestEmails() {

            // Construct a new email message and set data from test configuration in exe.config file
            var newEmail = new Mail();

            newEmail.From = this.configData.TestData.From;
            newEmail.ToAddresses = this.configData.TestData.To;
            newEmail.CcAddresses = this.configData.TestData.Cc;
            newEmail.BccAddresses = this.configData.TestData.Bcc;
            newEmail.Subject = this.configData.TestData.Subject;
            newEmail.SubjectPrefix = this.configData.TestData.SubjectPrefix;
            newEmail.Body = this.configData.TestData.Body;
            newEmail.HasAttachments = this.configData.TestData.HasAttachments;

            // Email is ready to be sent as-is if we don't have any attachments!
            newEmail.MarkMailAsReady = !this.configData.TestData.HasAttachments;

            // Save this email to DB
            this.dbConnect.SaveEmail(newEmail);

            // Add attachment data only if TestData had this configured
            if (newEmail.HasAttachments.GetValueOrDefault()) {

                // Add attachment
                var mailAttachment = new MailAttachment();
                mailAttachment.MailId = newEmail.MailId;
                mailAttachment.Filename = this.configData.TestData.Filename;

                // Sample image that is a concentric circle, to demonstrate that attachments work well!
                mailAttachment.Bytes = Convert.FromBase64String(this.configData.TestData.Attachment);

                mailAttachment.Filesize = (uint)mailAttachment.Bytes.Length;

                // Save attachment to DB
                dbConnect.SaveAttachment(mailAttachment);

                // Mark email as ready to be processed and sent!
                dbConnect.MarkEmailAsReady(newEmail.MailId.GetValueOrDefault());
            }

            // Construct a new email message and set data from test configuration in exe.config file
            newEmail = new Mail();

            newEmail.ToAddresses = this.configData.TestData.To;
            newEmail.Subject = this.configData.TestData.Subject;
            newEmail.Body = this.configData.TestData.Body;
            newEmail.HasAttachments = false;
            newEmail.Direct = true;
            newEmail.Timestamp = DateTime.UtcNow.AddMinutes(3);
            newEmail.Importance = true;

            // Email is ready to be sent as-is if we don't have any attachments!
            newEmail.MarkMailAsReady = true;

            // Save this email to DB
            this.dbConnect.SaveEmail(newEmail);
        }
        #endregion
        #endregion
    };
    #endregion
}
