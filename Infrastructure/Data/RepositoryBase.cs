using Domain.DomainBase;
using DomainBase;
using InfrastructureBase.Object;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.EfDataAccess
{
    public abstract class RepositoryBase<TContext, DomainModel, PersistenceObject> : IRepository<DomainModel> where TContext : DbContext where PersistenceObject : Entity where DomainModel : Entity
    {
        private readonly TContext context;
        public RepositoryBase(TContext context)
        {
            this.context = context;
        }
        public virtual DomainModel Add(DomainModel t)
        {
            t.CreateDate = DateTime.Now;
            t.CreateUserId = Common.GetCurrentUser().Id;
            t.LastUpdateDate = DateTime.Now;
            t.LastUpdateUserId = Common.GetCurrentUser().Id;
            var po = t.CopyTo<PersistenceObject>();
            context.Set<PersistenceObject>().Add(po);
            return po as DomainModel;
        }


        public virtual void Delete(DomainModel t)
        {
            t.IsDelete = true;
            var po = t.CopyTo<PersistenceObject>();
            po.LastUpdateDate = DateTime.Now;
            po.LastUpdateUserId = Common.GetCurrentUser().Id;
            context.Set<PersistenceObject>().Attach(po).State = EntityState.Deleted;
        }

        public virtual void Delete(Expression<Func<DomainModel, bool>> condition)
        {
            context.Set<PersistenceObject>().Where(condition.ReplaceParameter<DomainModel, PersistenceObject>()).ExecuteUpdate(x => x.SetProperty(
                s => s.IsDelete, true).SetProperty(s => s.LastUpdateDate, DateTime.Now).SetProperty(s => s.LastUpdateUserId, Common.GetCurrentUser().Id));
        }
        public virtual async Task DeleteAsync(Expression<Func<DomainModel, bool>> condition)
        {
            await context.Set<PersistenceObject>().Where(condition.ReplaceParameter<DomainModel, PersistenceObject>()).ExecuteUpdateAsync(x => x.SetProperty(
                s => s.IsDelete, true).SetProperty(s => s.LastUpdateDate, DateTime.Now).SetProperty(s => s.LastUpdateUserId, Common.GetCurrentUser().Id));
        }
        public virtual async Task<DomainModel> GetAsync(int key = 0, bool isDeleted = false)
        {
            PersistenceObject po;
            if (key == 0)
                return default;
            else
                po = await context.Set<PersistenceObject>().FindAsync(key);
            if (po == null || po.IsDelete != isDeleted)
                return default;
            context.Set<PersistenceObject>().Attach(po).State = EntityState.Detached;
            return po.CopyTo<DomainModel>();
        }
        public virtual async Task<DomainModel> GetAsync(Expression<Func<DomainModel, bool>> condition, bool isDeleted = false, List<SortedParams> sorteds = null)
        {
            PersistenceObject po = await context.Set<PersistenceObject>().Where(x => x.IsDelete == isDeleted).DataSort(sorteds).FirstOrDefaultAsync(condition.ReplaceParameter<DomainModel, PersistenceObject>());
            if (po == null)
                return default;
            context.Set<PersistenceObject>().Attach(po).State = EntityState.Detached;
            return po.CopyTo<DomainModel>();
        }
        public virtual async Task<List<DomainModel>> GetManyAsync(Expression<Func<DomainModel, bool>> condition, List<SortedParams> sorteds = null, bool isDeleted = false)
        {
            List<PersistenceObject> poResult = default;
            var query = context.Set<PersistenceObject>().AsNoTracking().Where(x => x.IsDelete == isDeleted).Where(condition.ReplaceParameter<DomainModel, PersistenceObject>()).DataSort(sorteds);
            poResult = await query.ToListAsync();
            var dolist = new List<DomainModel>();
            if (poResult.Any())
            {
                foreach (var po in poResult)
                {
                    dolist.Add(po.CopyTo<DomainModel>());
                }
            }
            return dolist;
        }
        public virtual async Task<List<DomainModel>> GetManyToListAsync(int[] key, bool isDeleted = false)
        {
            return await context.Set<PersistenceObject>().AsNoTracking().Where(x => x.IsDelete == isDeleted && key.ToList().Contains((x as Entity).Id)).Select(x => x.CopyTo<DomainModel>()).ToListAsync();
        }

        public virtual void Update(DomainModel t)
        {
            var po = t.CopyTo<PersistenceObject>();
            po.LastUpdateDate = DateTime.Now;
            po.LastUpdateUserId = Common.GetCurrentUser().Id;
            context.Set<PersistenceObject>().Attach(po).State = EntityState.Modified;
        }
        public virtual void Update<TProperty>(Expression<Func<DomainModel, bool>> condition, Expression<Func<DomainModel, TProperty>> setSetProperty, TProperty value)
        {
            var lambda = setSetProperty.ReplaceParameter<DomainModel, PersistenceObject, TProperty>().ExecuteUpdateExtension(value);
            context.Set<PersistenceObject>()
                .Where(condition.ReplaceParameter<DomainModel, PersistenceObject>())
                .ExecuteUpdate(lambda);
        }
        public virtual async Task<bool> AnyAsync(int key = 0, bool isDeleted = false)
        {
            if (key != 0)
                return await context.Set<PersistenceObject>().AnyAsync(x => x.IsDelete == isDeleted && (x as Entity).Id == (int)key);
            else
                return await context.Set<PersistenceObject>().AnyAsync();
        }

        public virtual async Task<bool> AnyAsync(Expression<Func<DomainModel, bool>> condition, bool isDeleted = false)
        {
            return await context.Set<PersistenceObject>().Where(x => x.IsDelete == isDeleted).Where(condition.ReplaceParameter<DomainModel, PersistenceObject>()).AnyAsync();
        }
        public virtual async Task<(int total, List<DomainModel> lists)> GetManyByPageAsync(Expression<Func<DomainModel, bool>> condition, int skip, int take, List<SortedParams> sorteds = null, bool isDeleted = false)
        {
            var poCount = await context.Set<PersistenceObject>().AsNoTracking().Where(x => x.IsDelete == isDeleted).CountAsync(condition.ReplaceParameter<DomainModel, PersistenceObject>());
            var dolist = new List<DomainModel>();
            if (poCount > 0)
            {
                List<PersistenceObject> poResult = default;
                var query = context.Set<PersistenceObject>().AsNoTracking().Where(x => x.IsDelete == isDeleted).Where(condition.ReplaceParameter<DomainModel, PersistenceObject>()).DataSort(sorteds);
                poResult = await query.Skip(skip).Take(take).ToListAsync();
                if (poResult.Any())
                {
                    foreach (var po in poResult)
                    {
                        dolist.Add(po.CopyTo<DomainModel>());
                    }
                }
            }
            return (poCount, dolist);
        }
    }
}
