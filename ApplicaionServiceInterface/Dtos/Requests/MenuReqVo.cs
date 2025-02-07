
using System.Collections.Generic;

namespace ApplicaionServiceInterface.Dtos.Requests
{
    /// <summary>
    /// 系统菜单展示
    /// </summary>
    public class MenuReqVo
    {
        /// <summary>
        /// 是否展示系统菜单
        /// </summary>
        public bool ShowSystem { get; set; }
        /// <summary>
        /// 是否需要展平
        /// </summary>
        public bool FlatMenu { get; set; }
        /// <summary>
        /// 需要查找的ID集合
        /// </summary>
        public List<int> Filters { get; set; }
    }
}
