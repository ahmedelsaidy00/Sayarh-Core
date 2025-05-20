using Abp.Domain.Entities;
using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Repositories;
namespace Sayarah.EntityFramework.Repositories
{
    public abstract class SayarahRepositoryBase<TEntity, TPrimaryKey> : EfCoreRepositoryBase<SayarahDbContext, TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        protected SayarahRepositoryBase(IDbContextProvider<SayarahDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }

        //add common methods for all repositories
    }

    public abstract class SayarahRepositoryBase<TEntity> : SayarahRepositoryBase<TEntity, int>
        where TEntity : class, IEntity<int>
    {
        protected SayarahRepositoryBase(IDbContextProvider<SayarahDbContext> dbContextProvider)
            : base(dbContextProvider)
        {

        }

        //do not add any method here, add to the class above (since this inherits it)
    }
}
