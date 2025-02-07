using ApplicaionServiceInterface.Dtos.Responses;
using Domain.Enums;
using System;
using System.Collections.Generic;

namespace ApplicaionServiceInterface.Dtos.Requests
{
    public class RoleListReqVo
    {
        /// <summary>
        /// 角色ID（可选）
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 角色拥有的菜单集合
        /// </summary>
        public List<MenuRespVo> Menus { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 角色类型
        /// </summary>
        public UserRoleType RoleType { get; set; }
        /// <summary>
        /// 最后更新用户
        /// </summary>
        public string LastUpdateUserName { get; set; }
        /// <summary>
        /// 最后更新日期
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }

        public RoleListReqVo()
        {
            Menus = new List<MenuRespVo>();
        }
    }
}
