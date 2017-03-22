/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * Partial class to provide Dispose and InitializeComponent methods.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Partial CorpMailmanService class
    /// <summary>
    /// Partial class to provide Dispose and InitializeComponent methods
    /// </summary>
    partial class CorpMailmanService {

        #region Members
        /// <summary>Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;
        #endregion

        #region Methods
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {

            if (disposing && (this.components != null)) {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {

            this.components = new System.ComponentModel.Container();
            this.ServiceName = Constants.System.NtServiceName;
        }
        #endregion
        #endregion
    }
    #endregion
}
