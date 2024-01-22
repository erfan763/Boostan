using DominClass.Entities.User;

namespace Application.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllUser();

    Task<User> CreateUser(User user);
}