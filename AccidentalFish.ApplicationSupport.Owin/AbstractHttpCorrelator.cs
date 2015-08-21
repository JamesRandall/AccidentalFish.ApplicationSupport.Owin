using System;
using Microsoft.Owin;

namespace AccidentalFish.ApplicationSupport.Owin
{
    /// <summary>
    /// Base class for middleware that requires a correlation ID
    /// </summary>
    public abstract class AbstractHttpCorrelator : OwinMiddleware
    {
        private readonly string _httpCorrelationHeaderKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">The next piece of OWIN middleware to invoke</param>
        /// <param name="httpCorrelationHeaderKey">The name of the header to use for the correlation ID</param>
        protected AbstractHttpCorrelator(OwinMiddleware next, string httpCorrelationHeaderKey) : base(next)
        {
            _httpCorrelationHeaderKey = httpCorrelationHeaderKey;
        }

        /// <summary>
        /// Adds the correlation ID to the request if not already present and ads it to the response
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The correlation ID that was already present or the one assigned</returns>
        protected string UseHttpTrackingIdInRequestAndResponse(IOwinContext context)
        {
            if (String.IsNullOrWhiteSpace(_httpCorrelationHeaderKey)) return null;

            Guid httpTrackingId;
            string[] httpTrackingIdAsString;
            IOwinRequest request = context.Request;
            if (request.Headers.TryGetValue(_httpCorrelationHeaderKey, out httpTrackingIdAsString))
            {
                httpTrackingId = Guid.Parse(httpTrackingIdAsString[0]);
            }
            else
            {
                httpTrackingId = Guid.NewGuid();
                request.Headers.Append(_httpCorrelationHeaderKey, httpTrackingId.ToString());
            }

            context.Response.OnSendingHeaders(responseCtx =>
            {
                IOwinContext headerContext = (IOwinContext)responseCtx;
                headerContext.Response.Headers.Set(_httpCorrelationHeaderKey, httpTrackingId.ToString());
            }, context);
            return httpTrackingId.ToString();
        }

        /// <summary>
        /// The name of the header to store the correlation ID in
        /// </summary>
        protected string HttpCorrelationHeaderKey => _httpCorrelationHeaderKey;
    }
}
