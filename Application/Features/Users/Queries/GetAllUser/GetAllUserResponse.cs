namespace Application.Features.Users.Queries.GetAllUser;

public sealed record GetAllUserResponse
{
    public string email { get; set; }
    public string name { get; set; }
}