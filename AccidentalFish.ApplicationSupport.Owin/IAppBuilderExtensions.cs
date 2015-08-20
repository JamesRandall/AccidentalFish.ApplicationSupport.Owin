using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Owin;

namespace AccidentalFish.ApplicationSupport.Owin
{
    // ReSharper disable once InconsistentNaming
    public static class IAppBuilderExtensions
    {
        /// <summary>
        /// In order to capture the full set of data during a HTTP request this OWIN plugin must be the first plugin in the chain.
        /// 
        /// Configure OWIN to trace HTTP requests and responses. Note that capturing request query parameters and request and response data could cause
        /// data security issues - potentially sensitive data will be logged to Azure table storage.
        /// 
        /// Therefore by default none of this is captured.
        /// 
        /// </summary>
        /// <param name="appBuilder">The app builder extended</param>
        /// <param name="loggerRepository">The repository to use for storing http logs</param>
        /// <param name="captureRequestParams">True if you wish to capture query parameters, false if not.</param>
        /// <param name="captureRequestData">True if you wish to capture request data, false if not.</param>
        /// <param name="captureResponseData">True if you wish to capture response data, falise if not.</param>
        /// <param name="captureRequestHeaders">To capture all request headers set a single array element of "*" otherwise specify the headers you wish to capture.</param>
        /// <param name="captureResponseHeaders">To capture all response headers set a single array element of "*" otherwise specify the headers you wish to capture.</param>
        /// <param name="httpCorrelationHeaderKey">
        /// In order to tie together request and response logging the http logger needs a correlation ID. By default this is added to request and response
        /// headers so that HTTP logging can be tied together across services and clients. The default name for this header is http-correlation-id. If a
        /// correlation ID is passed in in the header then it will be used, if no correlation ID is passed in then one will be generated.
        /// 
        /// This behaviour can be disabled by setting this parameter to null or an empty string.
        /// </param>
        /// <param name="azureTablePrefix">If you need to avoid table name conflicts</param>
        /// <returns></returns>
        public static IAppBuilder UseHttpLogger(this IAppBuilder appBuilder,
            IHttpLoggerRepository loggerRepository,
            bool captureRequestParams = false,
            bool captureRequestData = false,
            bool captureResponseData = false,
            string[] captureRequestHeaders = null,
            string[] captureResponseHeaders = null,
            string httpCorrelationHeaderKey = "http-correlation-id")
        {
            appBuilder.Use<HttpLogger>(
                loggerRepository,
                captureRequestParams,
                captureRequestData,
                captureResponseData,
                captureRequestHeaders,
                captureResponseHeaders,
                httpCorrelationHeaderKey);
            return appBuilder;
        }
    }
}
