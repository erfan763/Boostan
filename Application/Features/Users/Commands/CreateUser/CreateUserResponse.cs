namespace Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
}