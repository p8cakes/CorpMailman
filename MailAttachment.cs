/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * MailAttachment class to collect together all information related to an email attachment we need to tag on to an email.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    using System.Collections.Generic;
    #endregion

    #region MailAttachment class
    /// <summary>
    /// MailAttachment class to collect together all information related to an email attachment we need to tag on to an email
    /// </summary>
    internal class MailAttachment {

        #region Properties
        /// <summary>Mail ID</summary>
        internal uint? MailId { get; set; }

        /// <summary>File name</summary>
        internal string Filename { get; set; }

        /// <summary>File size</summary>
        internal uint? Filesize { get; set; }

        /// <summary>Bytes that actually contain the attachment</summary>
        internal byte[] Bytes { get; set; }
        #endregion
    }
    #endregion
}
