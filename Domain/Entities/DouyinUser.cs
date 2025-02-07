using DomainBase;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class DouyinUser : Entity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string OpenId { get; set; }
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string Avatar { get; set; }
    }
}
