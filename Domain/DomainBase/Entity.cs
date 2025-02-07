using System;
using System.ComponentModel.DataAnnotations;

namespace DomainBase
{
    /// <summary>
    /// 领域实体标记
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// key
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 逻辑删除
        /// </summary>
        public bool IsDelete { get; set; }
        /// <summary>
        /// 创建用户
        /// </summary>
        public int CreateUserId { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 最后更新用户
        /// </summary>
        public int? LastUpdateUserId { get; set; }
        /// <summary>
        /// 最后更新日期
        /// </summary>
        public DateTime? LastUpdateDate { get; set; }
    }
}
