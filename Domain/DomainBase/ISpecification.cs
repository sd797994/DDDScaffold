using System.Threading.Tasks;

namespace DomainBase
{
    public interface ISpecification<TEntity>
    {
        Task<bool> IsSatisfiedBy(TEntity entity);
    }
}
