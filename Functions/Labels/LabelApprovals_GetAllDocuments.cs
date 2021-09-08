using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;  
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juxce.Tuneage.Domain.TableEntities;
using Juxce.Tuneage.Domain;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels
{
    public static class LabelApprovals_GetAllDocuments
    {
        [FunctionName("LabelApprovals_GetAllDocuments")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] Label req,
            [Table("%TableName_LabelApprovals%")] CloudTable cloudTable, // Azure Table storage input binding, via TableAttribute
            ILogger log)
        {
            try {
                log.LogInformation($"LabelApprovals_GetAllDocuments function executed at: {DateTime.Now}");

                int numberOfResults = 0;
                Int32.TryParse(System.Environment.GetEnvironmentVariable("NumberOfResultsPerPage",
                                                                         EnvironmentVariableTarget.Process),
                    out numberOfResults
                );

                TableQuery<LabelTableEntity> getAllQuery = 
                    new TableQuery<LabelTableEntity>().Take(numberOfResults);

                List<Label> results = new List<Label>();
                foreach (LabelTableEntity entity in
                    await cloudTable.ExecuteQuerySegmentedAsync(getAllQuery, null)) {
                        results.Add(Mapper.LabelEntityToReturnObject(entity));
                    }

                return new OkObjectResult(JsonConvert.SerializeObject(results));
            }
            catch(Exception ex) {
                ErrorHandling.LogUnexpectedError(ex, log);
                return ErrorHandling.BuildCustomUnexpectedErrorObjectResult();
            }
        }
    }
}
