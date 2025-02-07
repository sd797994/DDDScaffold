using Domain.Enums;
using System;
using System.Collections.Generic;

namespace ApplicaionServiceInterface.Dtos.Responses
{
    /// <summary>
    /// 用户回调
    /// </summary>
    public class GetUserResp
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户真实名称
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 角色类型
        /// </summary>
        public UserRoleType RoleType { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 角色集合
        /// </summary>
        public List<UserRoleResp> RoleNameList { get; set; }
        /// <summary>
        /// 最后更新用户
        /// </summary>
        public string LastUpdateUserName { get; set; }
        /// <summary>
        /// 最后更新日期
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }
    }
    /// <summary>
    /// 用户角色集合
    /// </summary>
    public class UserRoleResp
    {
        /// <summary>
        /// 角色id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 角色名
        /// </summary>
        public string Name { get; set; }
    }
}
