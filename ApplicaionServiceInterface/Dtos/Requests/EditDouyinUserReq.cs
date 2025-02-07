using Domain.Enums;
namespace ApplicaionServiceInterface.Dtos.Requests
{

    /// <summary>
    /// 编辑抖音用户
    /// </summary>
    public class EditDouyinUserReq
    {
        /// <summary>
        /// id
        /// </summary>
        public int Id { get; set; }
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