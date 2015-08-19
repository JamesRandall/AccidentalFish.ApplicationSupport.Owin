﻿using Owin;

namespace AccidentalFish.ApplicationSupport.Owin.Azure
{
    /// <summary>
    /// Sets the level of granularity on the partition key of the httprequestbydatedescending table. Busier your site likely the more granular you want
    /// this to be.
    /// </summary>
    public enum LogByDateGranularityEnum
    {
        Day,
        Hour,
        Minute,
        Second
    }

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
        /// <param name="captureRequestHeaders"></param>
        /// <param name="captureResponseHeaders"></param>
        /// <param name="httpCorrelationHeaderKey">
        /// In order to tie together request and response logging the http logger needs a correlation ID. By default this is added to request and response
        /// headers so that HTTP logging can be tied together across services and clients. The default name for this header is http-correlation-id. If a
        /// correlation ID is passed in in the header then it will be used, if no correlation ID is passed in then one will be generated.
        /// 
        /// This behaviour can be disabled by setting this parameter to null or an empty string.
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
            string httpCorrelationHeaderKey = "http-correlation-id",
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