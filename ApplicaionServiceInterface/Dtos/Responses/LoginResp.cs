using Domain.Enums;
using System.Collections.Generic;

namespace ApplicaionServiceInterface.Dtos.Responses
{
    /// <summary>
    /// 用户登录回调
    /// </summary>
    public class LoginResp
    {
        public int Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 用户权限1 超级 2 普通
        /// </summary>
        public UserRoleType RoleType { get; set; }
        /// <summary>
        /// 用户状态1正常，2锁定
        /// </summary>
        public UserStatus Status { get; set; }
        /// <summary>
        /// 登录token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public List<int> UserRoles { get; set; }
    }
}
