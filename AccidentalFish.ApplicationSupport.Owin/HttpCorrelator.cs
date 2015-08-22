using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace AccidentalFish.ApplicationSupport.Owin
{
    /// <summary>
    /// Adds a correlation ID header to the request if not already present and also adds it to the response.
    /// </summary>
    public class HttpCorrelator : OwinMiddleware
    {
        private readonly string _httpCorrelationHeaderKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">The next piece of OWIN middleware to invoke</param>
        /// <param name="httpCorrelationHeaderKey">The name of the header to use for the correlation ID</param>
        public HttpCorrelator(OwinMiddleware next, string httpCorrelationHeaderKey) : base(next)
        {
            _httpCorrelationHeaderKey = httpCorrelationHeaderKey;
            if (string.IsNullOrWhiteSpace(httpCorrelationHeaderKey))
            {
                throw new ArgumentException("A correlation header name must be provided", nameof(httpCorrelationHeaderKey));
            }
        }

        /// <summary>
        /// Adds the correlation ID and invokes the  next middleware
        /// </summary>
        /// <param name="context">The owin context</param>
        /// <returns>A task</returns>
        public override async Task Invoke(IOwinContext context)
        {
            UseHttpTrackingIdInRequestAndResponse(context);
            await Next.Invoke(context);
        }

        /// <summary>
        /// Adds the correlation ID to the request if not already present and ads it to the response
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The correlation ID that was already present or the one assigned</returns>
        private string UseHttpTrackingIdInRequestAndResponse(IOwinContext context)
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
    }
}
