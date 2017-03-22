/****************************** Module Header ******************************
 * Module Name:  CorpMailman Windows Service project.
 * Project:      CorpMailman: Windows Service to send out emails project.
 *
 * EventCodes enumeration to describe event codes to be written to log.
 *
 * Revisions:
 *     1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
***************************************************************************/

namespace CorpMailman {

    #region EventCodes enumeration
    /// <summary>
    /// EventCodes enumeration to describe event codes to be written to log.
    ///     501: Normal, don't log to event or DB.
    ///     502: StoppingService, stopping the service.
    ///     503: VeryVerbose, log timer wake-ups to event-log.
    ///     504: CompletedTask - finished scheduled task.
    ///     505: FlagThreadpoolStoppage - NT Service thread attempting to flag down threadpools that may be auditing
    ///     506: EmptyEmail - No To: address, so time to purge this email
    ///     601: Error - something blew up spectacularly
    /// </summary>
    internal enum EventCodes {

        // Starting the Service.
        StartingService = 501,

        // Stopping the Service.
        StoppingService = 502,

        // StartingTask - starting new task.
        StartingTask = 503,

        // CompletedTask - finished scheduled task.
        CompletedTask = 504,

        // FlagThreadPoolStoppage - NT Service thread attempting to flag down threadpools that may be auditing
        FlagThreadPoolStoppage = 505,

        // EmptyEmail - no To: address, so this email would be deleted
        EmptyEmail = 506,

        // Error executing the task this time
        Error = 601
    }
    #endregion
}
