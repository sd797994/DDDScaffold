namespace ApplicaionServiceInterface.Dtos.Requests
{
    /// <summary>
    /// 修改角色入参
    /// </summary>
    public class EditRoleReq
    {
        /// <summary>
        /// 角色名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 角色ID
        /// </summary>
        public int? Id { get; set; }

    }
}