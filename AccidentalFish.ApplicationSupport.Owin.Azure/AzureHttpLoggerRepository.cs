using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccidentalFish.ApplicationSupport.Owin.Azure.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AccidentalFish.ApplicationSupport.Owin.Azure
{
    // TODO: This logger is currently somewhat naive. It undertakes its I/O operations during the HTTP request / response chain which
    // does add overhead, potentially significant if transient faults occur, to the response time. Consider batching / background updating.
    public class AzureHttpLoggerRepository : IHttpLoggerRepository
    {
        private const string RequestByCorrelationIdTableName = "httprequestbycorrelationid";
        private const string RequestByDateTimeDescendingTableName = "httprequestbydatedescending";
        private readonly CloudTable _byCorrelationIdTable;
        private readonly CloudTable _byDateTimeDescendingTable;
        private readonly string _granularPartitionKeyFormat;

        public AzureHttpLoggerRepository(string storageConnectionString, string azureTablePrefix, LogByDateGranularityEnum granularity)
        {
            if (string.IsNullOrWhiteSpace(storageConnectionString)) throw new ArgumentNullException(nameof(storageConnectionString));
            if (azureTablePrefix == null)
            {
                azureTablePrefix = "";
            }
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient client = storageAccount.CreateCloudTableClient();
            _byCorrelationIdTable = client.GetTableReference(string.Concat(azureTablePrefix, RequestByCorrelationIdTableName));
            _byDateTimeDescendingTable = client.GetTableReference(string.Concat(azureTablePrefix, RequestByDateTimeDescendingTableName));

            _byCorrelationIdTable.CreateIfNotExists();
            _byDateTimeDescendingTable.CreateIfNotExists();

            switch (granularity)
            {
                case LogByDateGranularityEnum.Hour:
                    _granularPartitionKeyFormat = "yyyy-MM-dd hh";
                    break;

                case LogByDateGranularityEnum.Day:
                    _granularPartitionKeyFormat = "yyyy-MM-dd";
                    break;

                case LogByDateGranularityEnum.Minute:
                    _granularPartitionKeyFormat = "yyyy-MM-dd hh:mm";
                    break;

                case LogByDateGranularityEnum.Second:
                    _granularPartitionKeyFormat = "yyyy-MM-dd hh:mm:ss";
                    break;
            }
        }

        public async Task Log(
            string uriToLog,
            bool didStripQueryParams,
            string httpCorrelationId,
            DateTimeOffset requestDateTime,
            long ellapsedMilliseconds,
            Dictionary<string, string[]> requestHeaders,
            Dictionary<string, string[]> responseHeaders)
        {
            Guid logItemId = Guid.NewGuid();

            Dictionary<string, EntityProperty> properties = CreateProperties(
                logItemId,
                uriToLog, 
                didStripQueryParams, 
                httpCorrelationId, 
                requestDateTime, 
                ellapsedMilliseconds, 
                requestHeaders,
                responseHeaders);

            DynamicTableEntity byCorrelationId = new DynamicTableEntity(
                HttpRequestLogItemByCorrelationId.FormatPartitionKey(httpCorrelationId),
                HttpRequestLogItemByCorrelationId.FormatRowKey(requestDateTime, logItemId),
                "*",
                properties);

            DynamicTableEntity byDateTimeDescending = new DynamicTableEntity(
                HttpRequestLogItemByDateTimeDescending.FormatPartitionKey(requestDateTime, _granularPartitionKeyFormat),
                HttpRequestLogItemByDateTimeDescending.FormatRowKey(httpCorrelationId, requestDateTime, logItemId),
                "*",
                properties);

            await Task.WhenAll(
                _byCorrelationIdTable.ExecuteAsync(TableOperation.Insert(byCorrelationId)),
                _byDateTimeDescendingTable.ExecuteAsync(TableOperation.Insert(byDateTimeDescending))
                );
        }

        private static Dictionary<string, EntityProperty> CreateProperties(Guid logItemId, string uriToLog, bool didStripQueryParams, string httpCorrelationId,
            DateTimeOffset requestDateTime, long ellapsedMilliseconds, Dictionary<string, string[]> requestHeaders, Dictionary<string, string[]> responseHeaders)
        {
            Dictionary<string, EntityProperty> requestProperties = new Dictionary<string, EntityProperty>
            {
                {"LogItemId", new EntityProperty(logItemId) },
                {"Url", new EntityProperty(uriToLog)},
                {"DidStripQueryParams", new EntityProperty(didStripQueryParams)},
                {"HttpCorrelationId", new EntityProperty(httpCorrelationId)},
                {"ElapsedMilliseconds", new EntityProperty(ellapsedMilliseconds)},
                {"RequestDateTime", new EntityProperty(requestDateTime)}
            };
            if (requestHeaders != null)
            {
                foreach (KeyValuePair<string, string[]> kvp in requestHeaders)
                {
                    string sanitizedHeaaderName = kvp.Key.Replace('-', '_');
                    if (kvp.Value.Length == 1)
                    {
                        requestProperties.Add($"RequestHeader_{sanitizedHeaaderName}", new EntityProperty(kvp.Value.Single()));
                    }
                    else
                    {
                        for (int headerIndex = 0; headerIndex < kvp.Value.Length; headerIndex++)
                        {
                            requestProperties.Add($"RequestHeader_{sanitizedHeaaderName}_{headerIndex}", new EntityProperty(kvp.Value[headerIndex]));
                        }
                    }
                }
            }

            if (responseHeaders != null)
            {
                foreach (KeyValuePair<string, string[]> kvp in responseHeaders)
                {
                    string sanitizedHeaaderName = kvp.Key.Replace('-', '_');
                    if (kvp.Value.Length == 1)
                    {
                        requestProperties.Add($"ResponseHeader_{sanitizedHeaaderName}", new EntityProperty(kvp.Value.Single()));
                    }
                    else
                    {
                        for (int headerIndex = 0; headerIndex < kvp.Value.Length; headerIndex++)
                        {
                            requestProperties.Add($"ResponseHeader_{sanitizedHeaaderName}_{headerIndex}", new EntityProperty(kvp.Value[headerIndex]));
                        }
                    }
                }
            }

            return requestProperties;
        }
    }
}
