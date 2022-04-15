using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juxce.Tuneage.Domain;
using Juxce.Tuneage.Domain.TableEntities;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels {
  public static class LabelApprovalsSimilarityCheck {
    [FunctionName("LabelApprovalsSimilarityCheck")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] Label req,
        [Table("%TableName_LabelApprovals%")] CloudTable cloudTable, // Azure Table storage input binding, via TableAttribute
        ILogger log) {
      try {
        log.LogInformation($"LabelApprovalsSimilarityCheck function executed for {req.ShortName} at: {DateTime.Now}");

        // Devise query that will search for anything that starts with the shortName
        // being used, and hack a query together that finds any existing entry that
        // begins with the same shortName, by incrementing the final character by
        // one and searching in between using ge and lt
        // (props: https://scotthelme.co.uk/hacking-table-storage-like-queries/)                
        string shortName = req.ShortName;
        if (string.IsNullOrEmpty(shortName))
          return new BadRequestObjectResult("No shortName was found in the request. Sorry.");
        string searchString = Utilities.MakeSearchString(shortName);
        char lastChar = searchString[searchString.Length - 1];
        lastChar++;
        char nextAsciiChar = lastChar;
        char[] phraseAsChars = searchString.ToCharArray();
        phraseAsChars[searchString.Length - 1] = nextAsciiChar;
        string shortNameUpperBound = new string(phraseAsChars);

        TableQuery<LabelTableEntity> sameFirstCharsQuery = new TableQuery<LabelTableEntity>().Where(
            TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("RowKey", "ge", searchString),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", "lt", shortNameUpperBound)
            )
        );

        List<Label> results = new List<Label>();
        foreach (LabelTableEntity entity in
            await cloudTable.ExecuteQuerySegmentedAsync(sameFirstCharsQuery, null)) {
          results.Add(Mapper.LabelEntityToReturnObject(entity));
        }

        return new OkObjectResult(JsonConvert.SerializeObject(results));
      }
      catch (Exception ex) {
        ErrorHandling.LogUnexpectedError(ex, log);
        return ErrorHandling.BuildCustomUnexpectedErrorObjectResult();
      }
    }
  }
}
