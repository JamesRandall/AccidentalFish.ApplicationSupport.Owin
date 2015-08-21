using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccidentalFish.ApplicationSupport.Owin
{
    /// <summary>
    /// Interface implemented by HTTP logger repositories.
    /// </summary>
    public interface IHttpLoggerRepository
    {
        /// <summary>
        /// Logs the information to a store
        /// </summary>
        /// <param name="uriToLog">The URI to log. Will contain query parameters if didStripQueryParams is false.</param>
        /// <param name="didStripQueryParams">Have query parameters been stripped from the URI.</param>
        /// <param name="httpCorrelationId">The correlation ID if available</param>
        /// <param name="requestDateTime">The date and time the request arrived at the OWIN middleware</param>
        /// <param name="ellapsedMilliseconds">The time taken to process the request (the time between entering the middleware and exiting it)</param>
        /// <param name="requestHeaders">Any request headers and their values</param>
        /// <param name="responseHeaders">Any response headers and their values</param>
        /// <returns>Task</returns>
        Task Log(
            string uriToLog,
            bool didStripQueryParams,
            string httpCorrelationId,
            DateTimeOffset requestDateTime,
            long ellapsedMilliseconds,
            Dictionary<string,string[]> requestHeaders,
            Dictionary<string, string[]> responseHeaders);
    }
}
