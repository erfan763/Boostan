using Boostan.Application.Features.Users.Queries.GenerateUserTokenByPassword;
using Boostan.WebFramework.BaseController;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Boostan.Controllers;

[ApiController]
[Route("user")]
public class UserController : BaseController
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    //[HttpGet("get_all")]
    //public async Task<ActionResult<List<GetAllUserResponse>>> GetAll(CancellationToken cancellationToken)
    //{
    //    var response = await _mediator.Send(new GetAllUserRequest(), cancellationToken);
    //    return Ok(response);
    //}


    //[HttpPost("create")]
    //public async Task<ActionResult<CreateUserResponse>> Create(CreateUserRequest request,
    //    CancellationToken cancellationToken)
    //{
    //    var response = await _mediator.Send(request, cancellationToken);
    //    return Ok(response);
    //}


    [HttpPost("login")]
    public async Task<IActionResult> Login(GenerateUserTokenByPasswordQuery request)
    {
        var result = await _mediator.Send(request);
        return OperationResult(result);
    }


    //private IActionResult OperationResult(OperationResult<AccessToken> result)
    //{
    //    throw new NotImplementedException();
    //}
}