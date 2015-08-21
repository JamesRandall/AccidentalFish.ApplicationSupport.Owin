using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace AccidentalFish.ApplicationSupport.Owin
{
    public sealed class HttpLogger : AbstractHttpCorrelator
    {
        private readonly IHttpLoggerRepository _httpLoggerRepository;
        private readonly bool _captureRequestParams;
        private readonly bool _captureRequestData;
        private readonly bool _captureResponseData;
        private readonly string[] _captureRequestHeaders;
        private readonly string[] _captureResponseHeaders;
        
        public HttpLogger(OwinMiddleware next,
            IHttpLoggerRepository httpLoggerRepository,
            bool captureRequestParams,
            bool captureRequestData,
            bool captureResponseData,
            string[] captureRequestHeaders,
            string[] captureResponseHeaders,
            string httpCorrelationHeaderKey) : base(next, httpCorrelationHeaderKey)
        {
            _httpLoggerRepository = httpLoggerRepository;
            _captureRequestParams = captureRequestParams;
            _captureRequestData = captureRequestData;
            _captureResponseData = captureResponseData;
            _captureRequestHeaders = captureRequestHeaders;
            _captureResponseHeaders = captureResponseHeaders;
        }

        public override async Task Invoke(IOwinContext context)
        {
            IOwinRequest request = context.Request;
            IOwinResponse response = context.Response;

            DateTimeOffset requestTime = DateTimeOffset.UtcNow;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string httpCorrelationId = Guid.NewGuid().ToString();

            if (!string.IsNullOrWhiteSpace(HttpCorrelationHeaderKey))
            {
                httpCorrelationId = UseHttpTrackingIdInRequestAndResponse(context);
            }

            await Next.Invoke(context);

            sw.Stop();
            string uriToLog = request.Uri.ToString();
            bool didStripQueryParams = false;
            if (!_captureRequestParams && request.QueryString.HasValue)
            {
                uriToLog = uriToLog.Substring(0, uriToLog.Length - request.QueryString.Value.Length - 1);
                didStripQueryParams = true;
            }

            Dictionary<string, string[]> requestHeaders = CaptureHeaders(_captureRequestHeaders, request.Headers);
            Dictionary<string, string[]> responseHeaders = CaptureHeaders(_captureResponseHeaders, response.Headers);

            await _httpLoggerRepository.Log(uriToLog, didStripQueryParams, httpCorrelationId, requestTime, sw.ElapsedMilliseconds, requestHeaders, responseHeaders);
        }

        private Dictionary<string, string[]> CaptureHeaders(IReadOnlyCollection<string> captureHeaders, IHeaderDictionary headers)
        {
            Dictionary<string, string[]> capturedHeaders = null;
            if (captureHeaders != null && captureHeaders.Any())
            {
                capturedHeaders = new Dictionary<string, string[]>();
                if (captureHeaders.First() == "*")
                {
                    foreach (KeyValuePair<string, string[]> kvp in headers)
                    {
                        string[] existingHeader;
                        if (capturedHeaders.TryGetValue(kvp.Key, out existingHeader))
                        {
                            existingHeader = existingHeader.Union(kvp.Value).ToArray();
                        }
                        else
                        {
                            existingHeader = kvp.Value;
                        }
                        capturedHeaders[kvp.Key] = existingHeader;
                    }
                }
                else
                {
                    foreach (string headerName in captureHeaders)
                    {
                        string[] values;
                        if (headers.TryGetValue(headerName, out values))
                        {
                            capturedHeaders.Add(headerName, values);
                        }
                    }
                }
            }
            return capturedHeaders;
        } 
    }
}
