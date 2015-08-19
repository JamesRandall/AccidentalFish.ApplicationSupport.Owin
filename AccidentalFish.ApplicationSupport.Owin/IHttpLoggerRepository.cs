using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccidentalFish.ApplicationSupport.Owin
{
    public interface IHttpLoggerRepository
    {
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
