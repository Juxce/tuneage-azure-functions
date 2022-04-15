using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juxce.Tuneage.Domain.TableEntities;
using Juxce.Tuneage.Domain;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels {
  public static class GetLabelApproval {
    [FunctionName("GetLabelApproval")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] LabelTableEntity req,
        [Table("%TableName_LabelApprovals%", "{ShortName}", "{RowKey}")] LabelTableEntity labelEntity, // Azure Table storage input binding, via TableAttribute
        ILogger log) {
      try {
        log.LogInformation($"GetLabelApproval function executed at: {DateTime.Now}");

        string shortName = req.ShortName;

        if (string.IsNullOrEmpty(shortName))
          return new BadRequestObjectResult("No shortName was found in the request. Sorry.");
        if (labelEntity == null) {
          log.LogError($"Label Approval request for invalid shortName: {shortName}");
          return new ObjectResult(new { error = "The shortName requested does not exist. Oof!" }) {
            StatusCode = StatusCodes.Status500InternalServerError
          };
        }

        Label returnLabel = new Label {
          ShortName = labelEntity.ShortName,
          LongName = labelEntity.LongName,
          Url = labelEntity.Url,
          Profile = labelEntity.Profile
        };

        return new OkObjectResult(JsonConvert.SerializeObject(returnLabel));
      }
      catch (Exception ex) {
        ErrorHandling.LogUnexpectedError(ex, log);
        return ErrorHandling.BuildCustomUnexpectedErrorObjectResult();
      }
    }
  }
}
