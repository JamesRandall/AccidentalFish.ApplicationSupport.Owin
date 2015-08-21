using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace AccidentalFish.ApplicationSupport.Owin
{
    public abstract class AbstractHttpCorrelator : OwinMiddleware
    {
        private readonly string _httpCorrelationHeaderKey;

        protected AbstractHttpCorrelator(OwinMiddleware next, string httpCorrelationHeaderKey) : base(next)
        {
            _httpCorrelationHeaderKey = httpCorrelationHeaderKey;
        }

        protected string UseHttpTrackingIdInRequestAndResponse(IOwinContext context)
        {
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

        protected string HttpCorrelationHeaderKey => _httpCorrelationHeaderKey;
    }
}
