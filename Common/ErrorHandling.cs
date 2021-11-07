using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Juxce.Tuneage.Common {
  public class ErrorHandling {
    public static void LogUnexpectedError(Exception ex, ILogger log) {
      log.LogError(ex, Messages.UnexpectedError);
    }

    public static ObjectResult BuildCustomUnexpectedErrorObjectResult() {
      return new ObjectResult(new { error = Messages.UserFriendlyUnexpectedError }) {
        StatusCode = StatusCodes.Status500InternalServerError
      };
    }
  }
}