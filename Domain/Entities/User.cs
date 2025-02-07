using Domain.Enums;
using DomainBase;

namespace Domain.Entities
{
    public class User : Entity
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /** 1 超级 2 普通 */
        public UserRoleType RoleType { get; set; }
        /** 1正常，2锁定 */
        public int Status { get; set; }
    }
}
