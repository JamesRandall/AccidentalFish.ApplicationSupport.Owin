using Owin;

namespace AccidentalFish.ApplicationSupport.Owin
{
    /// <summary>
    /// Extends the IAppBuilder with extensions for adding a HTTP logger and HTTP correlation ID manager
    /// </summary>
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
        /// <param name="appBuilder">The OWIN app builder</param>
        /// <param name="loggerRepository">The repository to use for storing http logs</param>
        /// <param name="captureRequestParams">True if you wish to capture query parameters, false if not.</param>
        /// <param name="captureRequestData">True if you wish to capture request data, false if not.</param>
        /// <param name="captureResponseData">True if you wish to capture response data, falise if not.</param>
        /// <param name="captureRequestHeaders">To capture all request headers set a single array element of "*" otherwise specify the headers you wish to capture.</param>
        /// <param name="captureResponseHeaders">To capture all response headers set a single array element of "*" otherwise specify the headers you wish to capture.</param>
        /// <param name="httpCorrelationHeaderKey">
        /// It can be helpful when calling across http boundaries to be able to tie together the flow of events with a correlation ID and by default the logger
        /// looks for a correlation ID in the header correlation-id. If the header is missing then no correlation ID is used but the logger will work. If you
        /// wish to disable this behaviour then set this to null or if you want to use a different header then set the header name here.
        /// 
        /// The HttpCorrelator middleware also in this assembly can be used to add a correlation ID if none is present and should be placed before the logger middleware
        /// in the pipeline.
        /// </param>
        /// <returns></returns>
        public static IAppBuilder UseHttpLogger(this IAppBuilder appBuilder,
            IHttpLoggerRepository loggerRepository,
            bool captureRequestParams = false,
            bool captureRequestData = false,
            bool captureResponseData = false,
            string[] captureRequestHeaders = null,
            string[] captureResponseHeaders = null,
            string httpCorrelationHeaderKey = "correlation-id")
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

        /// <summary>
        /// Use this to add a correlation ID to your http responses and requests to enable the correlation of events
        /// across multiple services and clients.
        /// 
        /// By default the correlation ID is stored in a header with the name correlation-id but this can be changed
        /// with the httpCorrelationHeaderKey parameter.
        /// 
        /// If the request has an existing correlation header then this is added to the request, if no header is present
        /// then a GUID is generated and used as the ID and added to both the request and the response.
        /// </summary>
        /// <param name="appBuilder">The OWIN app builder</param>
        /// <param name="httpCorrelationHeaderKey">The name of the header to store the correlation ID in.</param>
        /// <returns></returns>
        public static IAppBuilder UseHttpCorrelator(this IAppBuilder appBuilder,
            string httpCorrelationHeaderKey = "correlation-id")
        {
            appBuilder.Use<HttpCorrelator>(httpCorrelationHeaderKey);
            return appBuilder;
        }
    }
}
