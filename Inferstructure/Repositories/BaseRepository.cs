using Inferstructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Inferstructure.Repositories;

public abstract class BaseRepository<TEntitiy> where TEntitiy : class
{
    private readonly AppDbContext appDbContext;

    protected BaseRepository(AppDbContext context)
    {
        appDbContext = context;
        Entities = appDbContext.Set<TEntitiy>();
    }


    protected DbSet<TEntitiy> Entities { get; }

    protected virtual async Task<TEntitiy> Create(TEntitiy entity)
    {
        Entities.Add(entity);
        return entity;
    }

    protected virtual async Task<TEntitiy> Update(TEntitiy entity)
    {
        Entities.Update(entity);
        return entity;
    }

    protected virtual async Task<string> Delete(TEntitiy entity)
    {
        Entities.Update(entity);
        return "deleted successfully";
    }

    protected virtual async Task<List<TEntitiy>> GetAll()
    {
        return await Entities.ToListAsync();
    }
}