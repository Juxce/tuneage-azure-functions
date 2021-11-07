using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Juxce.Tuneage.Domain.TableEntities;
using Juxce.Tuneage.Domain;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels
{
  public static class LabelApprovals_CreateDocument
  {
    [FunctionName("LabelApprovals_CreateDocument")]
    [return: Table("%TableName_LabelApprovals%")] // Azure Table storage output binding, via TableAttribute
    public static LabelTableEntity Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] Label req,
        ILogger log)
    {
      try
      {
        log.LogInformation($"LabelApprovals_CreateDocument function processed for: {req.ShortName}");

        return new LabelTableEntity
        {
          PartitionKey = Utilities.SanitizePrimaryKey(req.ShortName),
          RowKey = Utilities.MakeSearchString(req.ShortName),
          ShortName = req.ShortName,
          LongName = req.LongName,
          Url = req.Url,
          Profile = req.Profile
        };
      }
      catch (Exception ex)
      {
        ErrorHandling.LogUnexpectedError(ex, log);
        return new LabelTableEntity { };
      }
    }
  }
}
