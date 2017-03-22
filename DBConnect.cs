/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * DBConnect class to interface with MySQL backend and run procedures.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using MySql.Data.MySqlClient;
    #endregion

    #region DBConnect class
    /// <summary>
    /// DBConnect class - interface with MySQL backend and run procedures
    /// </summary>
    internal class DBConnect {

        #region Members
        /// <summary>MySqlConnection to DB</summary>
        private MySqlConnection connection;
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        internal DBConnect() {
            this.Initialize();
        }
        #endregion

        #region Methods
        #region Public/internal Methods
        /// <summary>
        /// Get Next Mail to send out
        /// </summary>
        internal Mail GetNextMailToSend() {

            var mail = null as Mail;

            if (this.OpenConnection()) {

                string query = Constants.Table.Queries.GetEmailToSend;

                // Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                // Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                // Read the data and store them in the list
                if (dataReader.Read()) {

                    mail = new Mail();

                    mail.MailId = dataReader[Constants.Table.Mails.MailId] as uint?;
                    mail.From = dataReader[Constants.Table.Mails.Sender] as string;
                    mail.ToAddresses = dataReader[Constants.Table.Mails.Recipients] as string;
                    mail.CcAddresses = dataReader[Constants.Table.Mails.CcRecipients] as string;
                    mail.BccAddresses = dataReader[Constants.Table.Mails.BccRecipients] as string;
                    mail.Subject = dataReader[Constants.Table.Mails.Subject] as string;
                    mail.SubjectPrefix = dataReader[Constants.Table.Mails.SubjectPrefix] as string;
                    mail.Body = dataReader[Constants.Table.Mails.Body] as string;
                    mail.HasAttachments = (dataReader[Constants.Table.Mails.HasAttachments] as byte?).GetValueOrDefault() == 1;
                    mail.Created = (DateTime)dataReader[Constants.Table.Mails.Created];
                    mail.Direct = (dataReader[Constants.Table.Mails.Direct] as byte?).GetValueOrDefault() == 1;
                    mail.Importance = (dataReader[Constants.Table.Mails.Importance] as byte?).GetValueOrDefault() == 1;
                }

                //close Connection
                this.CloseConnection();
            }

            // Read attachments for this mail, if you have any!
            if ((mail != null) && (mail.HasAttachments.GetValueOrDefault())) {

                if (this.OpenConnection()) {

                    string query = Constants.Table.Queries.GetAttachmentsForEmail;

                    // Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.MailId, mail.MailId);

                    // Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    // Read the data and store them in the list
                    while (dataReader.Read()) {

                        var attachment = new MailAttachment();

                        attachment.MailId = dataReader[Constants.Table.Mails.MailId] as uint?;
                        attachment.Filename = dataReader[Constants.Table.Attachments.Filename] as string;
                        attachment.Filesize = dataReader[Constants.Table.Attachments.Filesize] as uint?;
                        attachment.Bytes = dataReader[Constants.Table.Attachments.Attachment] as byte[];

                        mail.AddAttachment(attachment);
                    }

                    //close Connection
                    this.CloseConnection();
                }
            }

            return mail;
        }

        /// <summary>
        /// Save this email to database
        /// </summary>
        /// <param name="mail">Mail object</param>
        internal void SaveEmail(Mail mail) {

            if (this.OpenConnection()) {

                var query = Constants.Table.Queries.AddEmail;

                // Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Sender, mail.Direct ? DBNull.Value : mail.From as object);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Recipients, mail.ToAddresses);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.CcRecipients, string.IsNullOrEmpty(mail.CcAddresses) ? DBNull.Value : mail.CcAddresses as object);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.BccRecipients, string.IsNullOrEmpty(mail.BccAddresses) ? DBNull.Value : mail.BccAddresses as object);

                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Subject, mail.Subject);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.SubjectPrefix, string.IsNullOrEmpty(mail.SubjectPrefix) ? DBNull.Value : mail.SubjectPrefix as object);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Body, mail.Body);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.MarkMailAsReady, mail.MarkMailAsReady);

                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.HasAttachments, mail.HasAttachments);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Importance, mail.Importance);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Direct, mail.Direct);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Timestamp, mail.Timestamp.HasValue ? mail.Timestamp.GetValueOrDefault() as object : DBNull.Value);

                // Execute Scalar - don't bother reading the mail attachment ID
                mail.MailId = (uint)((cmd.ExecuteScalar() as ulong?).GetValueOrDefault());

                //close Connection
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Get Next Mail to send out
        /// </summary>
        /// <param name="attachment">MailAttachment object</param>
        internal void SaveAttachment(MailAttachment attachment) {

            if (this.OpenConnection()) {

                string query = Constants.Table.Queries.AddMailAttachment;

                // Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.MailId, attachment.MailId);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Filename, attachment.Filename);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Filesize, attachment.Filesize);
                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.Attachment, attachment.Bytes);

                // Execute Scalar - don't bother reading the mail attachment ID
                cmd.ExecuteScalar();

                //close Connection
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Mark email as ready to be processed in the Database
        /// </summary>
        /// <param name="mailId">Mail ID</param>
        internal void MarkEmailAsReady(uint mailId) {

            if (this.OpenConnection()) {

                string query = Constants.Table.Queries.MarkEmailAsReady;

                // Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.MailId, mailId);

                // Execute Non-Query
                cmd.ExecuteNonQuery();

                //close Connection
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Delete this email from Database
        /// </summary>
        /// <param name="mailId">Mail ID</param>
        internal void DeleteEmail(uint mailId) {

            if (this.OpenConnection()) {

                string query = Constants.Table.Queries.DeleteEmail;

                // Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue(Constants.Table.Queries.Parameters.MailId, mailId);

                // Execute Non-Query
                cmd.ExecuteNonQuery();

                //close Connection
                this.CloseConnection();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initialize DBConnect instance
        /// </summary>
        private void Initialize() {

            // Construct MySqlConnection instance, specified by connection string
            this.connection = new MySqlConnection(ConfigData.Instance.ConnectionString);
        }

        /// <summary>
        /// Open connection
        /// </summary>
        /// <returns>True if connection succeeded, false otherwise</returns>
        private bool OpenConnection() {

            bool connected = false;

            try {

                this.connection.Open();

                connected = true;
            } catch (MySqlException ex) {

                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number) {

                    case 0:
                        // Fail if you cannot connect to the backend database
                        throw new CorpMailmanException(Constants.Messages.Database.CannotConnect, ex);

                    case 1045:
                        // Fail if the username password combination is invalid to the database
                        throw new CorpMailmanException(Constants.Messages.Database.InvalidCredentials, ex);
                }
            }

            return connected;
        }

        /// <summary>
        /// Close connection
        /// </summary>
        private void CloseConnection() {

            this.connection.Close();
        }
        #endregion
        #endregion
    }
    #endregion
}