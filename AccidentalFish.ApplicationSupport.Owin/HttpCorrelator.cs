using System.Threading.Tasks;
using Microsoft.Owin;

namespace AccidentalFish.ApplicationSupport.Owin
{
    public class HttpCorrelator : AbstractHttpCorrelator
    {
        public HttpCorrelator(OwinMiddleware next, string httpCorrelationHeaderKey) : base(next, httpCorrelationHeaderKey)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            UseHttpTrackingIdInRequestAndResponse(context);
            await Next.Invoke(context);
        }
    }
}
