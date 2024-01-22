using System.Security.Claims;
using Boostan.Application.Models.Common;
using Dotnet.fs.WebFramework.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Boostan.WebFramework.BaseController;

public class BaseController : ControllerBase
{
    private string Port => HttpContext.Request.Host.Port is not null
        ? $":{HttpContext.Request.Host.Port}"
        : string.Empty;

    protected string AppBaseUrl =>
        $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Host}{Port}{HttpContext.Request.PathBase}";

    protected string UserKey => User.FindFirstValue(ClaimTypes.UserData);

    protected void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
    }

    protected IActionResult OperationResult<TModel>(OperationResult<TModel> result)
    {
        if (result is null)
            return new ServerErrorResult("Server Error");


        if (result.IsSuccess) return result.Result is bool ? Ok() : Ok(result.Result);

        if (result.IsNotFound)
        {
            ModelState.AddModelError("GeneralError", result.ErrorMessage);

            var notFoundErrors = new ValidationProblemDetails(ModelState);

            return NotFound(notFoundErrors.Errors);
        }

        switch (result.CustomCode)
        {
            case 403:
                ModelState.AddModelError("Forbidden", result.ErrorMessage);
                return StatusCode(result.CustomCode);
            case > 0:
                ModelState.AddModelError("GatewayError", result.ErrorMessage);
                return StatusCode(result.CustomCode);
        }

        ModelState.AddModelError("GeneralError", result.ErrorMessage);

        var badRequestErrors = new ValidationProblemDetails(ModelState);
        return BadRequest(badRequestErrors.Errors);
    }
}