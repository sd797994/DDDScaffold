using Domain.IRespoitory;
using Infrastructure.DataBase;
using Infrastructure.DataBase.PO;
using Infrastructure.EfDataAccess;

namespace Infrastructure.Repository
{
    public class PermissionRepository : RepositoryBase<MySqlEfContext, Domain.Entities.Permission, Permission>, IPermissionRepository
    {
        public PermissionRepository(MySqlEfContext sqlEfContext) : base(sqlEfContext) { }
    }
}