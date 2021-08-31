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
    public static class LabelApprovals_SimilarityCheck
    {
        [FunctionName("LabelApprovals_SimilarityCheck")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] Label req,
            [Table("%TableName_LabelApprovals%")] CloudTable cloudTable, // Azure Table storage input binding, via TableAttribute
            ILogger log)
        {
            try
            {
                log.LogInformation($"LabelApprovals_SimilarityCheck function executed for {req.ShortName} at: {DateTime.Now}");

                // Devise query that will search for anything that starts with the shortName
                // being used, and hack a query together that finds any existing entry that
                // begins with the same shortName, by incrementing the final character by
                // one and searching in between using ge and lt
                // (props: https://scotthelme.co.uk/hacking-table-storage-like-queries/)                
                string shortName = req.ShortName;
                if (string.IsNullOrEmpty(shortName))
                    return new BadRequestObjectResult("No shortName was found in the request. Sorry.");
                char lastChar = shortName[shortName.Length - 1];
                char nextAsciiChar = lastChar++;
                char[] phraseAsChars = shortName.ToCharArray();
                phraseAsChars[shortName.Length - 1] = nextAsciiChar;
                string shortNameUpperBound = phraseAsChars.ToString();

                TableQuery<LabelTableEntity> sameFirstCharsQuery = new TableQuery<LabelTableEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", "ge", shortName),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("PartitionKey", "lt", shortNameUpperBound)
                    )
                );

                string responseMessage = string.Empty;
                foreach (LabelTableEntity entity in
                    await cloudTable.ExecuteQuerySegmentedAsync(sameFirstCharsQuery, null)) {
                        responseMessage += JsonConvert.SerializeObject(
                            Mapper.LabelEntityToReturnObject(entity)
                        );
                    }

                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                ErrorHandling.LogUnexpectedError(ex, log);
                return ErrorHandling.BuildCustomUnexpectedErrorObjectResult();
            }
        }
    }
}
