using Domain.IRespoitory;
using Infrastructure.DataBase;
using Infrastructure.DataBase.PO;
using Infrastructure.EfDataAccess;

namespace Infrastructure.Repository
{
    public class RoleRepository : RepositoryBase<MySqlEfContext, Domain.Entities.Role, Role>, IRoleRepository
    {
        public RoleRepository(MySqlEfContext sqlEfContext) : base(sqlEfContext) { }
    }
}