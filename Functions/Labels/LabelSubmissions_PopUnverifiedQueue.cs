using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Juxce.Tuneage.Common;

namespace Juxce.Tuneage.Functions.Labels
{
    public static class LabelSubmissions_PopUnverifiedQueue
    {
        [FunctionName("LabelSubmissions_PopUnverifiedQueue")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("LabelSubmissions_PopUnverifiedQueue function processed a request.");

                string messageId = req.Query["messageId"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                messageId = messageId ?? data?.messageId;

                if (!string.IsNullOrEmpty(messageId)) // && messageId = messageId
                {
                    // Create QueueClient to use proper Base64 encoding to match Azure Functions default
                    string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage",
                                                                                 EnvironmentVariableTarget.Process);
                    string queueName = Environment.GetEnvironmentVariable("QueueName_LabelSubmissionsUnverified",
                                                                          EnvironmentVariableTarget.Process);
                    QueueClient queue = new QueueClient(connectionString,
                                                        queueName,
                                                        new QueueClientOptions {
                                                            MessageEncoding = QueueMessageEncoding.Base64
                                                        });
                    await queue.CreateAsync();

                    // Pop the next message in queue
                    var result = await queue.ReceiveMessageAsync();
                    var nextMessage = result.Value;
                    if (nextMessage.MessageId != messageId) {
                        return new BadRequestObjectResult("Requested messageId did not match the next message in queue.");
                    }
                    await queue.DeleteMessageAsync(nextMessage.MessageId, nextMessage.PopReceipt);
                } else {
                    return new BadRequestObjectResult("No messageId was found in the request. Sorry.");
                }

                return new OkObjectResult("OK, deleted the top message.");
            }
            catch (Exception ex)
            {
                ErrorHandling.LogUnexpectedError(ex, log);
                return ErrorHandling.BuildCustomUnexpectedErrorObjectResult();
            }
        }
    }
}
