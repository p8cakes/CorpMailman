/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * Mail class to collect together all information related to an email we need to send.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    #region Mail class
    /// <summary>
    /// Mail class to collect together all information related to an email we need to send
    /// </summary>
    internal class Mail {

        #region Members
        /// <summary>List of </summary>
        private List<MailAttachment> attachments;

        /// <summary>List of Target To: Addresses</summary>
        private List<string> toAddresses;

        /// <summary>List of CC: Addresses, if any</summary>
        private List<string> ccAddresses;

        /// <summary>List of BCC: Addresses, if any</summary>
        private List<string> bccAddresses;
        #endregion

        #region Constructor
        internal Mail() {
            this.attachments = new List<MailAttachment>();
        }
        #endregion

        #region Properties
        /// <summary>Mail ID</summary>
        internal uint? MailId { get; set; }

        /// <summary>From Address</summary>
        internal string From { get; set; }

        /// <summary>Direct</summary>
        internal bool Direct { get; set; }

        /// <summary>Importance</summary>
        internal bool Importance { get; set; }

        /// <summary>Subject</summary>
        internal string Subject { get; set; }

        /// <summary>Created timestamp (usually UTC)</summary>
        internal DateTime? Created { get; set; }

        /// <summary>Mail send timestamp value (should be UTC)</summary>
        internal DateTime? Timestamp { get; set; }

        /// <summary>Subject Prefix</summary>
        internal string SubjectPrefix { get; set; }

        /// <summary>Whether this has any attachments</summary>
        internal bool? HasAttachments { get; set; }

        /// <summary>Body</summary>
        internal string Body { get; set; }

        /// <summary>Mark mail as ready to send</summary>
        internal bool MarkMailAsReady { get; set; }

        /// <summary>To Addresses, read from database</summary>
        internal string ToAddresses {
            get {
                var toAddress = null as string;

                if (this.toAddresses != null) {
                    var builder = new StringBuilder();

                    foreach (var address in this.toAddresses) {

                        if (builder.Length > 0) {
                            builder.Append(Constants.Comma);
                        }

                        builder.Append(address);
                    }

                    toAddress = builder.ToString();
                    builder.Remove(0, builder.Length);
                }

                return toAddress;
            }
            set {
                if (this.toAddresses == null) {
                    this.toAddresses = new List<string>();

                    var toAddressArray = value.Split(Constants.CommaArray);

                    foreach (var toAddress in toAddressArray) {
                        this.toAddresses.Add(toAddress.Trim());
                    }
                }
            }
        }

        /// <summary>CC Addresses, read from database</summary>
        internal string CcAddresses {
            get {
                var ccAddress = null as string;

                if (this.ccAddresses != null) {
                    var builder = new StringBuilder();

                    foreach (var address in this.ccAddresses) {

                        if (builder.Length > 0) {
                            builder.Append(Constants.Comma);
                        }

                        builder.Append(address);
                    }

                    ccAddress = builder.ToString();
                    builder.Remove(0, builder.Length);
                }

                return ccAddress;
            }
            set {
                if ((this.ccAddresses == null) && (value != null)) {
                    this.ccAddresses = new List<string>();

                    var ccAddressArray = value.Split(Constants.CommaArray);

                    foreach (var ccAddress in ccAddressArray) {
                        this.ccAddresses.Add(ccAddress.Trim());
                    }
                }
            }
        }

        /// <summary>BCC Addresses, read from database</summary>
        internal string BccAddresses {
            get {
                var bccAddress = null as string;

                if (this.bccAddresses != null) {
                    var builder = new StringBuilder();

                    foreach (var address in this.bccAddresses) {

                        if (builder.Length > 0) {
                            builder.Append(Constants.Comma);
                        }

                        builder.Append(address);
                    }

                    bccAddress = builder.ToString();
                    builder.Remove(0, builder.Length);
                }

                return bccAddress;
            }
            set {
                if ((this.bccAddresses == null) && (value != null)) {
                    this.bccAddresses = new List<string>();

                    var bccAddressArray = value.Split(Constants.CommaArray);

                    foreach (var bccAddress in bccAddressArray) {
                        this.bccAddresses.Add(bccAddress.Trim());
                    }
                }
            }
        }

        /// <summary>Get the list of addresses we need to send this mail out to</summary>
        internal List<string> ToAddressList { get { return this.toAddresses; } }

        /// <summary>Get the list of cc: addresses we need to send this mail out to (if any)</summary>
        internal List<string> CcAddressList { get { return this.ccAddresses; } }

        /// <summary>Get the list of bcc: addresses we need to send this mail out to (if any)</summary>
        internal List<string> BccAddressList { get { return this.bccAddresses; } }

        /// <summary>Gets the list of MailAttachments</summary>
        internal List<MailAttachment> Attachments { get { return this.attachments; } }
        #endregion

        #region Methods
        #region Public/internal Methods
        /// <summary>
        /// Add the MailAttachment to list of attachments
        /// </summary>
        /// <param name="attachment">MailAttachment instance</param>
        internal void AddAttachment(MailAttachment attachment) {
            this.attachments.Add(attachment);
        }
        #endregion
        #endregion
    }
    #endregion
}
