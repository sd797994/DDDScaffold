using Domain.Enums;
using DomainBase;

namespace Domain.Entities
{
    public class Permission : Entity
    {
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MenuName { get; set; }
        /// <summary>
        /// 父ID
        /// </summary>
        public int? Pid { get; set; }
        /// <summary>
        /// 菜单key
        /// </summary>
        public string MenuPage { get; set; }
        /// <summary>
        /// 菜单类型
        /// </summary>
        public PermissionMenuType MenuType { get; set; }
        /// <summary>
        /// 菜单Icon
        /// </summary>
        public string MenuIcon { get; set; }
        /// <summary>
        /// 菜单状态
        /// </summary>
        public PermissionStatus Status { get; set; }
        /// <summary>
        /// 菜单描述
        /// </summary>
        public string MenuDisplayInfo { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 是否是系统菜单
        /// </summary>
        public bool ShowSystem { get; set; }
    }
}
