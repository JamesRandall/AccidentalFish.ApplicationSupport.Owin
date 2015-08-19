using System;

namespace AccidentalFish.ApplicationSupport.Owin.Azure.Model
{
    internal static class HttpRequestLogItemByCorrelationId
    {
        public static string FormatPartitionKey(string httpCorrelationId)
        {
            return httpCorrelationId;
        }

        public static string FormatRowKey(DateTimeOffset requestDateTime, Guid logItemId)
        {
            return $"{ requestDateTime.Ticks}_{logItemId}";
        }
    }
}
