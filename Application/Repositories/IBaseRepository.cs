using DominClass.Common.BaseEtity;

namespace Application.Repositories;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    void Create(TEntity entity);
    void Update(TEntity entity);

    void Delete(TEntity entity);

    Task<TEntity> Get(Guid id, CancellationToken cancellationToken);

    Task<List<TEntity>> GetAll(CancellationToken cancellationToken);
}