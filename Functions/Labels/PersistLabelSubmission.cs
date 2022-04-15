using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juxce.Tuneage.Domain.TableEntities;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels {
  public static class PersistLabelSubmission {
    [FunctionName("PersistLabelSubmission")]
    [return: Table("%TableName_LabelSubmissions%")] // Azure Table storage output binding, via TableAttribute
    public static LabelTableEntity Run(
        [QueueTrigger("%QueueName_LabelSubmissionsInput%")] string queueItem, // Azure Queue storage input binding and trigger, via QueueTriggerAttribute
        [Queue("%QueueName_LabelSubmissionsUnverified%")] ICollector<string> msg, // Azure Queue storage output binding, via QueueAttribute
        ILogger log) {
      log.LogInformation($"PersistLabelSubmission function processed: {queueItem}");

      // Write same queueItem to secondary queue for unverified label submissions
      msg.Add(queueItem);

      // Deserialize input queueItem, then build and return new table entry
      dynamic data = JsonConvert.DeserializeObject(queueItem);
      return new LabelTableEntity {
        PartitionKey = Utilities.SanitizePrimaryKey(data.shortName.ToString()),
        RowKey = Utilities.GetTicks(),
        ShortName = data.shortName,
        LongName = data.longName,
        Url = data.url,
        Profile = data.profile
      };
    }
  }
}
