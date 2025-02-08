using ApplicaionServiceInterface.Dtos.Bases;
using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Interface.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicaionServiceInterface.Interface
{
    [ActionGenerator("角色管理")]
    public interface IRoleApplicationService
    {
        [ActionGeneratorMethod(RequestType.Post, "保存角色信息", "edit", true)]
        Task SaveRole(EditRoleReqVo input);

        [ActionGeneratorMethod(RequestType.Post, "删除角色", "delete", true)]
        Task DeleteRole(DeleteModelReq input);

        [ActionGeneratorMethod(RequestType.GET, "获取分页角色", "list", true)]
        Task<PageQueryResonseBase<RoleListReqVo>> GetAllRoleByPage(PageQueryInputBase input);

        [ActionGeneratorMethod(RequestType.GET, "获取所有角色", "all", true)]
        Task<List<RoleListReqVo>> GetAllRole();

        [ActionGeneratorMethod(RequestType.Post, "批量修改角色状态", "batchstatus", true)]
        Task BatchStatusRole(IdListStatusReqVo input);
    }
}