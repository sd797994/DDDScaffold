using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicaionServiceInterface.Dtos.Bases
{
    public class PageQueryInputBase
    {
        public int PageNum { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int GetSkip() => (PageNum - 1) * PageSize;
    }
    public class PageQueryKeywordsInputBase: PageQueryInputBase
    {
        /// <summary>
        /// 搜索关键字
        /// </summary>
        public string KeyWords { get; set; }
        /// <summary>
        /// 搜索近N天
        /// </summary>
        public int? PassDay { get; set; }
        /// <summary>
        /// 是否排序，null不排，true 正序 false倒序
        /// </summary>
        public bool? Sort { get; set; }
        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortName { get; set; }
    }
}
