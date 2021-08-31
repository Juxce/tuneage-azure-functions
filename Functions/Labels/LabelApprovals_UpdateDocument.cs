using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juxce.Tuneage.Domain;
using Juxce.Tuneage.Domain.TableEntities;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels
{
    public static class LabelApprovals_UpdateDocument
    {
        [FunctionName("LabelApprovals_UpdateDocument")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] Label req,
            [Table("%TableName_LabelApprovals%")] CloudTable cloudTable, // Azure Table storage input binding, via TableAttribute
            ILogger log)
        {
            try
            {
                log.LogInformation($"LabelApprovals_UpdateDocument function executed for {req.ShortName} at: {DateTime.Now}");

                string shortName = req.ShortName;
                if (string.IsNullOrEmpty(shortName))
                    return new BadRequestObjectResult("No shortName was found in the request. Sorry.");

                var labelEntity = new LabelTableEntity {
                    PartitionKey = shortName,
                    RowKey = string.Empty,
                    ShortName = shortName,
                    LongName = req.LongName,
                    Url = req.Url,
                    Profile = req.Profile
                };
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(labelEntity);
                TableResult result = await cloudTable.ExecuteAsync(insertOrMergeOperation);
                LabelTableEntity insertedLabel = result.Result as LabelTableEntity;
                if (result.RequestCharge.HasValue)
                    log.LogInformation($"Request Charge of InsertOrMerge operation: {result.RequestCharge}");

                return new OkObjectResult(JsonConvert.SerializeObject(insertedLabel));
            }
            catch (Exception ex)
            {
                ErrorHandling.LogUnexpectedError(ex, log);
                return ErrorHandling.BuildCustomUnexpectedErrorObjectResult();
            }
        }
    }
}
