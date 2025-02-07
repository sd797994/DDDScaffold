using Domain.IRespoitory;
using Infrastructure.DataBase;
using Infrastructure.DataBase.PO;
using Infrastructure.EfDataAccess;

namespace Infrastructure.Repository
{
    public class DouyinUserRepository : RepositoryBase<MySqlEfContext, Domain.Entities.DouyinUser, DouyinUser>, IDouyinUserRepository
    {
        public DouyinUserRepository(MySqlEfContext sqlEfContext) : base(sqlEfContext) { }
    }
}