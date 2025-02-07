using Domain.IRespoitory;
using Infrastructure.DataBase;
using Infrastructure.DataBase.PO;
using Infrastructure.EfDataAccess;

namespace Infrastructure.Repository
{
    public class UserRepository : RepositoryBase<MySqlEfContext, Domain.Entities.User, User>, IUserRepository
    {
        public UserRepository(MySqlEfContext sqlEfContext) : base(sqlEfContext) { }
    }
}
