using MediatR;

namespace Application.Features.Users.Queries.GetAllUser;

public sealed record GetAllUserRequest : IRequest<List<GetAllUserResponse>>;