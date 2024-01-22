using MediatR;

namespace Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserRequest(string Email, string Name) : IRequest<CreateUserResponse>;