using Domain.Enums;
using System.Collections.Generic;

namespace ApplicaionServiceInterface.Dtos.Requests
{
    /// <summary>
    /// 保存用户入参
    /// </summary>
    public class EditUserReq
    {
        /// <summary>
        /// id
        /// </summary>
        public int? Id { get; set; }
        /// <summary>
        /// 用户账号
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
        /// <summary>
        /// 角色类型
        /// </summary>
        public UserRoleType RoleType { get; set; }
        /// <summary>
        /// 用户状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public List<int> RoleIdList { get; set; }
    }
}
