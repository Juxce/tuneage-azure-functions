using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juxce.Tuneage.Domain.TableEntities;

namespace Juxce.Tuneage.Functions.Labels
{
    public static class LabelSubmissions_InputQueueToTable
    {
        [FunctionName("LabelSubmissions_InputQueueToTable")]
        [return: Table("%TableName_LabelSubmissions%")] // Azure Table storage output binding, via TableAttribute
        public static LabelTableEntity Run(
            [QueueTrigger("%QueueName_LabelSubmissionsInput%")]string queueItem, // Azure Queue storage input binding and trigger, via QueueTriggerAttribute
            [Queue("%QueueName_LabelSubmissionsUnverified%")] ICollector<string> msg, // Azure Queue storage output binding, via QueueAttribute
            ILogger log)
        {
            log.LogInformation($"LabelSubmissions_InputQueueToTable function processed: {queueItem}");
            
            // Write same queueItem to secondary queue for unverified label submissions
            msg.Add(queueItem);

            // Deserialize input queueItem, then build and return new table entry
            dynamic data = JsonConvert.DeserializeObject(queueItem);
            return new LabelTableEntity {
                PartitionKey = data.shortName,
                RowKey = string.Empty,
                ShortName = data.shortName,
                LongName = data.longName,
                Url = data.url,
                Profile = data.profile
            };
        }
    }
}
