using ApplicaionServiceInterface.Dtos.Bases;
using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Dtos.Responses;
using ApplicaionServiceInterface.Interface.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicaionServiceInterface.Interface
{
    [ActionGenerator("权限管理")]
    public interface IPermissionApplicationService
    {
        [ActionGeneratorMethod(RequestType.Post, "保存菜单", "edit", true)]
        Task SavePermission(EditPermissionReq input);

        [ActionGeneratorMethod(RequestType.Post, "删除菜单", "delete", true)]
        Task DeletePermission(DeleteModelReq input);

        [ActionGeneratorMethod(RequestType.GET, "获取所有菜单", "list", true)]
        Task<List<MenuRespVo>> GetAllPermission(MenuReqVo input);

        [ActionGeneratorMethod(RequestType.GET, "获取分页菜单", "page", true)]
        Task<PageQueryResonseBase<MenuRespVo>> GetPermissionByPage(PageQueryInputBase input);
    }
}