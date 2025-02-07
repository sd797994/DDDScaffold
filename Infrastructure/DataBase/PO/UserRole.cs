using InfrastructureBase.Data;

namespace Infrastructure.DataBase.PO
{
    public class UserRole : PersistenceObjectBase
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
