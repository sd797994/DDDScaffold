using InfrastructureBase.Data;

namespace Infrastructure.DataBase.PO
{
    public class RolePermission : PersistenceObjectBase
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }
}
