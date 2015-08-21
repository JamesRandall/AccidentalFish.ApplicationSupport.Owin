using System;

namespace AccidentalFish.ApplicationSupport.Owin.Azure.Model
{
    internal static class HttpRequestLogItemByDateTimeDescending
    {
        public static string FormatRowKey(string httpCorrelationId, DateTimeOffset requestDateTime, Guid logItemId)
        {
            if (httpCorrelationId == null)
            {
                httpCorrelationId = "";
            }
            
            return $"{DateTime.MaxValue.Ticks - requestDateTime.Ticks:D19} {httpCorrelationId}_{logItemId}";
        }

        public static string FormatPartitionKey(DateTimeOffset requestDateTime, string granularityFormat)
        {

            return requestDateTime.ToString(granularityFormat);
        }
    }
}
