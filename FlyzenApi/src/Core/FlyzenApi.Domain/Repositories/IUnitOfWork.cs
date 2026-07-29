namespace FlyzenApi.Domain.Repositories
{
    public interface IUnitOfWork
    {
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    }
}
