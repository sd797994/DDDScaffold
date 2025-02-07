using Domain.Enums;
using DomainBase;

namespace Domain.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Role : Entity
    {
        /// <summary>
        /// 角色名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 角色状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 角色类型
        /// </summary>
        public UserRoleType RoleType { get; set; }
    }
}
