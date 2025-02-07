

using ApplicaionServiceInterface.Dtos.Bases;
using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Dtos.Responses;
using ApplicaionServiceInterface.Interface.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicaionServiceInterface.Interface
{
    [ActionGenerator("用户管理")]
    public interface IUserApplicationService
    {
        [ActionGeneratorMethod(RequestType.Post, "用户登录", "login", false)]
        Task<ApiResult<LoginResp>> GetUserInfo(LoginReq input);

        [ActionGeneratorMethod(RequestType.Post, "保存用户信息", "edit", false)]
        Task<ApiResult> SaveUserInfo(EditUserReq input);

        [ActionGeneratorMethod(RequestType.Post, "获取登录用户信息", "info", true)]
        Task<ApiResult<GetUserResp>> UserInfo(GetModelReq input);

        [ActionGeneratorMethod(RequestType.Post, "批量修改用户状态", "batchstatus", true)]
        Task<ApiResult> BatchStatusRole(IdListStatusReqVo input);

        [ActionGeneratorMethod(RequestType.GET, "获取分页用户", "list", true)]
        Task<ApiResult<PageQueryResonseBase<GetUserResp>>> GetAllUserByPage(PageQueryInputBase input);

        [ActionGeneratorMethod(RequestType.GET, "获取用户的菜单", "menus", true)]
        Task<ApiResult<List<MenuRespVo>>> GetRoleMenus(GetModelReq input);

        [ActionGeneratorMethod(RequestType.Post, "删除用户", "delete", true)]
        Task<ApiResult> DeleteUser(DeleteModelReq input);
    }
}
