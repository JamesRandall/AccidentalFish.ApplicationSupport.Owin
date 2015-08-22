using Owin;

namespace AccidentalFish.ApplicationSupport.Owin.Azure
{
    /// <summary>
    /// Sets the level of granularity on the partition key of the httprequestbydatedescending table. Busier your site likely the more granular you want
    /// this to be.
    /// </summary>
    public enum LogByDateGranularityEnum
    {
        /// <summary>
        /// Group the partition key by day
        /// </summary>
        Day,
        /// <summary>
        /// Group the partition key by hour
        /// </summary>
        Hour,
        /// <summary>
        /// Group the partition key by minute
        /// </summary>
        Minute,
        /// <summary>
        /// Group the partition key by second
        /// </summary>
        Second
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// IAppBuilder extension methods for configuring the HTTP logger with the Azure repository
    /// </summary>
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
        /// The HTTP traces are stored in these tables:
        /// 
        ///     httprequestbydatedescending
        ///     httprequestbycorrelationid
        /// 
        /// If this causes you any conflicts then the table names can be given an optional prefix using the azurePrefix parameter.
        /// 
        /// Request and response data is stored in blob containers named httprequestdata and httpresponsedata respectively and the azurePrefix
        /// parameter is also applied to these container names. Blob names are linked to the log item ID held within the tables.
        /// </summary>
        /// <param name="appBuilder">The app builder extended</param>
        /// <param name="storageConnectionString">The storage string for </param>
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
        /// <param name="azurePrefix">If you need to avoid table name conflicts</param>
        /// <param name="granularity">Sets the level of granularity on the partition key of the httprequestbydatedescending table. Busier your site likely the more granular you want
        /// this to be. Defaults to hourly</param>
        /// <returns></returns>
        public static IAppBuilder UseAzureHttpLogger(this IAppBuilder appBuilder,
            string storageConnectionString,
            bool captureRequestParams = false,
            bool captureRequestData = false,
            bool captureResponseData = false,
            string[] captureRequestHeaders = null,
            string[] captureResponseHeaders = null,
            string httpCorrelationHeaderKey = "correlation-id",
            string azurePrefix = "",
            LogByDateGranularityEnum granularity = LogByDateGranularityEnum.Hour)
        {
            appBuilder.Use(typeof(HttpLogger),
                new AzureHttpLoggerRepository(storageConnectionString, azurePrefix, granularity),
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
