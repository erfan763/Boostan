using Boostan.Application.Models.ApiResult;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet.fs.WebFramework.Filters;

public class ServerErrorResult : IActionResult
{
    public ServerErrorResult(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var response = new ApiResult(Message);
        await context.HttpContext.Response.WriteAsJsonAsync(response);
    }
}