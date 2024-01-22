using AutoMapper;
using DominClass.Entities.User;

namespace Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserMapper : Profile


{
    public CreateUserMapper()
    {
        CreateMap<CreateUserRequest, User>();
        CreateMap<User, CreateUserRequest>();
    }
}