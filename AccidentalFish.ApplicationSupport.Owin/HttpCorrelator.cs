using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace AccidentalFish.ApplicationSupport.Owin
{
    /// <summary>
    /// Adds a correlation ID header to the request if not already present and also adds it to the response.
    /// </summary>
    public class HttpCorrelator : AbstractHttpCorrelator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">The next piece of OWIN middleware to invoke</param>
        /// <param name="httpCorrelationHeaderKey">The name of the header to use for the correlation ID</param>
        public HttpCorrelator(OwinMiddleware next, string httpCorrelationHeaderKey) : base(next, httpCorrelationHeaderKey)
        {
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
    }
}
