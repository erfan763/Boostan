using AutoMapper;
using DominClass.Entities.User;

namespace Application.Features.Users.Queries.GetAllUser;

public sealed class GetAllUserMapper : Profile
{
    public GetAllUserMapper()
    {
        CreateMap<User, GetAllUserResponse>();
    }
}