namespace ApplicaionServiceInterface.Dtos.Requests
{
    /// <summary>
    /// 请求用户登录入参
    /// </summary>
    public class LoginReq
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }
    }
}
