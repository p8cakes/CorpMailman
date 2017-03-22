/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * LogLevels enumeration for all levels we need to write events to System log.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region LogLevels enumeration
    /// <summary>
    /// Enumerate levels of logging configured.
    ///     0: Invalid - do not use.
    ///     1: Normal, don't log to event or DB.
    ///     1: Verbose, log starts and stops to event-log.
    ///     2: VeryVerbose, log timer wake-ups to event-log.
    /// </summary>
    internal enum LogLevels {

        // Invalid - do not use.
        Invalid = 0,

        // Normal - don't log to event or DB.
        Normal = 1,

        // Verbose - log starts and stops to event log.
        Verbose = 2,

        // VeryVerbose - log timer wake-ups to event log.
        VeryVerbose = 3
    }
    #endregion
}
