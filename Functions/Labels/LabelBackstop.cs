using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juxce.Tuneage.Domain;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels
{
  public static class LabelBackstop
  {
    [FunctionName("LabelBackstop")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [Queue("%QueueName_LabelSubmissionsInput%")] ICollector<string> msg, // Azure Queue storage output binding, via QueueAttribute
        ILogger log)
    {
      try
      {
        log.LogInformation("LabelBackstop function processed a request.");

        string shortName = req.Query["shortName"];
        string longName = req.Query["longName"];
        string url = req.Query["url"];
        string profile = req.Query["profile"];

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        shortName = shortName ?? data?.shortName;
        longName = longName ?? data?.longName;
        url = url ?? data?.url;
        profile = profile ?? data?.profile;

        if (!string.IsNullOrEmpty(shortName))
        {
          // Add a message to the output collection saving the Label data to the queue
          Label labelSubmission = new Label
          {
            ShortName = shortName,
            LongName = longName,
            Url = url,
            Profile = profile
          };
          string serializedLabel = JsonConvert.SerializeObject(labelSubmission);
          msg.Add(serializedLabel);
        }

        string responseMessage = string.IsNullOrEmpty(shortName)
            ? "Please pass at least a shortName in the query string or request body to make a submission."
            : $"The label {shortName} has been received. Thanks for your interest!";

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
