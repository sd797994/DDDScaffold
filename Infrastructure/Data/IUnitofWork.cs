namespace Infrastructure.EfDataAccess
{
    public interface IUnitofWork
    {
        Task ExecuteTransactionAsync(Func<Task> func);
        Task CommitAsync();
        Task ExecuteTransaction(Action dbFunc);
    }
}