using Application.Repositories;
using DominClass.Entities.User;
using Inferstructure.Context;

namespace Inferstructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly AppDbContext appDbContext;

    public UserRepository(AppDbContext context) : base(context)
    {
        appDbContext = context;
    }

    public Task<User> CreateUser(User user)
    {
        return base.Create(user);
    }

    public Task<List<User>> GetAllUser()
    {
        return base.GetAll();
    }


    //public Task<User> GetByEmail(string email)
    //{
    //    return appDbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
    //}
}