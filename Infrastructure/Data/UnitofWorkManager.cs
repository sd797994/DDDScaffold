using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EfDataAccess
{
    public class UnitofWorkManager<TContext> : IUnitofWork where TContext : DbContext
    {
        private readonly TContext context;
        public UnitofWorkManager(TContext context)
        {
            this.context = context;
        }
        public async Task CommitAsync()
        {
            await context.SaveChangesAsync();
        }
        public async Task ExecuteTransaction(Func<Task> dbFunc)
        {
            using var tran = await context.Database.BeginTransactionAsync();
            try
            {
                await dbFunc();
                await context.SaveChangesAsync();
                await tran.CommitAsync();
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task ExecuteTransaction(Action dbFunc)
        {
            using var tran = await context.Database.BeginTransactionAsync();
            try
            {
                dbFunc();
                await context.SaveChangesAsync();
                await tran.CommitAsync();
            }
            catch (Exception)
            {
                tran.Rollback();
                throw;
            }
        }
    }
}
