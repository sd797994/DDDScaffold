using ApplicaionServiceInterface.Dtos.Bases;
using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Dtos.Responses;
using ApplicaionServiceInterface.Interface.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicaionServiceInterface.Interface
{
    [ActionGenerator("抖音用户管理")]
    public interface IDouyinUserApplicationService
    {
        [ActionGeneratorMethod(RequestType.GET, "获取抖音用户", "getinfo", true)]
        Task<ApiResult<GetDouyinUserResp>> GetDouyinUserInfo(GetModelReq input);
        [ActionGeneratorMethod(RequestType.Post,"保存抖音用户", "edit", true)]
        Task<ApiResult> SaveDouyinUser(EditDouyinUserReq input);
        [ActionGeneratorMethod(RequestType.GET, "获取抖音用户分页", "page", true)]
        Task<ApiResult<PageQueryResonseBase<GetDouyinUserResp>>> GetDouyinUserByPage(PageQueryInputBase input);
        [ActionGeneratorMethod(RequestType.Post, "删除抖音用户", "delete", true)]
        Task<ApiResult> DeleteDouyinUser(DeleteModelReq input);
    }
}