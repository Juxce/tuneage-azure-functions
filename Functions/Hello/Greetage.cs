using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Juxce.Tuneage.Functions.Hello {
  public static class Greetage {
    [FunctionName("Greetage")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log) {
      log.LogInformation("Greetage function processed a request.");

      string name = req.Query["name"];

      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
      dynamic data = JsonConvert.DeserializeObject(requestBody);
      name = name ?? data?.name;

      string responseMessage = string.IsNullOrEmpty(name)
          ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response, ya Juxcehead."
          : $"Hello, {name}. This HTTP triggered function executed successfully. Now drink your Juxce.";

      return new OkObjectResult(responseMessage);
    }
  }
}
