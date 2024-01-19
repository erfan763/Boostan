using Application.Repositories;
using Inferstructure.Context;

namespace Inferstructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _appDbContext;

    public UnitOfWork(AppDbContext context)
    {
        _appDbContext = context;
    }

    public Task Save(CancellationToken cancellationToken)
    {
        return _appDbContext.SaveChangesAsync(cancellationToken);
    }
}