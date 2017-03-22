/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * ServiceInstaller class, to install this as an NT/Windows Service.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region Using directives
    using System;
    using System.ComponentModel;
    using System.Configuration.Install;
    #endregion

    #region ServiceInstaller class
    /// <summary>
    /// ServiceInstaller class, to install this as an NT/Windows Service.
    /// </summary>
    [RunInstaller(true)]
    public partial class ServiceInstaller : Installer {

        #region Constructor
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ServiceInstaller() {
            InitializeComponent();
        }
        #endregion
    }
    #endregion
}
