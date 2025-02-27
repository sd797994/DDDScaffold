namespace Infrastructure.EfDataAccess
{
    public interface IUnitofWork
    {
        Task ExecuteTransaction(Func<Task> func);
        Task CommitAsync();
        Task ExecuteTransaction(Action dbFunc);
    }
}