using Domain.Enums;
namespace ApplicaionServiceInterface.Dtos.Requests
{
    /// <summary>
    /// 修改权限入参
    /// </summary>
    public class EditPermissionReq
    {
        /// <summary>
        /// 菜单名
        /// </summary>
        public string MenuName { get; set; }
        /// <summary>
        /// 父id
        /// </summary>
        public int? Pid { get; set; }
        /// <summary>
        /// 菜单页
        /// </summary>
        public string MenuPage { get; set; }
        /// <summary>
        /// 菜单类型
        /// </summary>
        public PermissionMenuType MenuType { get; set; }
        /// <summary>
        /// icon
        /// </summary>
        public string MenuIcon { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public PermissionStatus Status { get; set; }
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 菜单描述
        /// </summary>
        public string MenuDisplayInfo { get; set; }
        /// <summary>
        /// 是否是系统菜单
        /// </summary>
        public bool ShowSystem { get; set; }
    }
}