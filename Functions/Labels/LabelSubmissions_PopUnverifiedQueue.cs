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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("LabelSubmissions_PopUnverifiedQueue function processed a request.");

                string messageId = req.Query["messageId"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                messageId = messageId ?? data?.messageId;

                if (!string.IsNullOrEmpty(messageId))
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
                    var peekResult = await queue.PeekMessageAsync(new System.Threading.CancellationToken());

                    var peekedMessage = peekResult.Value;
                    if (peekedMessage.MessageId != messageId) {
                        return new BadRequestObjectResult("Requested messageId did not match the next message in queue.");
                    }

                    var receiveMessageResult = await queue.ReceiveMessageAsync();

                    var nextMessage = receiveMessageResult.Value;
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
