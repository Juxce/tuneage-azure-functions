using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels
{
  public static class LabelSubmissions_PeekAtUnverifiedQueue
  {
    [FunctionName("LabelSubmissions_PeekAtUnverifiedQueue")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
      try
      {
        log.LogInformation("LabelSubmissions_PeekAtUnverifiedQueue function processed a request.");

        // Create QueueClient to use proper Base64 encoding to match Azure Functions default
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage",
                                                                     EnvironmentVariableTarget.Process);
        string queueName = Environment.GetEnvironmentVariable("QueueName_LabelSubmissionsUnverified",
                                                              EnvironmentVariableTarget.Process);
        QueueClient queue = new QueueClient(connectionString,
                                            queueName,
                                            new QueueClientOptions
                                            {
                                              MessageEncoding = QueueMessageEncoding.Base64
                                            });
        await queue.CreateAsync();

        // Peek at next message in queue
        var result = await queue.PeekMessageAsync(new System.Threading.CancellationToken());
        var peekedMessage = result.Value;

        return new OkObjectResult(peekedMessage.MessageText);
      }
      catch (Exception ex)
      {
        ErrorHandling.LogUnexpectedError(ex, log);
        return ErrorHandling.BuildCustomUnexpectedErrorObjectResult();
      }
    }
  }
}
