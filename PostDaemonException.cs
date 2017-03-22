/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * CorpMailmanException extends Exception for our application..
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    #endregion

    #region CorpMailmanException class
    /// <summary>
    /// CorpMailmanException extends Exception for our application
    /// </summary>
    internal class CorpMailmanException : Exception {

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        internal CorpMailmanException() {
        }

        /// <summary>
        /// CorpMailmanException constructed with message
        /// </summary>
        /// <param name="message">Exception message</param>
        internal CorpMailmanException(string message)
            : base(message) {
        }

        /// <summary>
        /// CorpMailmanException constructed with message and innerException
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner Exception instance</param>
        internal CorpMailmanException(string message, Exception innerException)
            : base(message, innerException) {
        }
        #endregion
    }
    #endregion
}
