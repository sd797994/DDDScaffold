using ApplicaionServiceInterface.Dtos.Bases;
using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Dtos.Responses;
using ApplicaionServiceInterface.Interface;
using Domain.Entities;
using Domain.IRespoitory;
using Infrastructure.EfDataAccess;
using InfrastructureBase;
using InfrastructureBase.Object;

namespace ApplicationServiceImpl
{
    public class DouyinUserApplicationService : IDouyinUserApplicationService
    {
        private readonly IDouyinUserRepository douyinuserRepository;
        private readonly IUnitofWork unitofWork;
        public DouyinUserApplicationService(IDouyinUserRepository douyinuserRepository, IUnitofWork unitofWork)
        {
            this.douyinuserRepository = douyinuserRepository;
            this.unitofWork = unitofWork;
        }
        public async Task<ApiResult<GetDouyinUserResp>> GetDouyinUserInfo(GetModelReq input)
        {
            if (input != null && input.Id != 0)
            {
                var douyinuser = await douyinuserRepository.GetAsync(input.Id);
                if (douyinuser != null)
                {
                    return ApiResult<GetDouyinUserResp>.Ok(douyinuser.CopyTo<DouyinUser, GetDouyinUserResp>());
                }
            }
            throw new ApplicationServiceException("没有找到对应的记录,请确定id是否正确");
        }

        public async Task<ApiResult> SaveDouyinUser(EditDouyinUserReq input)
        {
            if (input == null)
            {
                throw new ApplicationServiceException("没有传递有效的数据，无法进行记录增加/更新");
            }
            await unitofWork.ExecuteTransactionAsync(async () => {
                if (input.Id != 0)
                {
                    var douyinuser = await douyinuserRepository.GetAsync(input.Id);
                    if (douyinuser != null)
                    {
                        //按照实际情况更新信息
                        douyinuser.OpenId = input.OpenId;
                    douyinuser.NickName = input.NickName;
                    douyinuser.Avatar = input.Avatar;

                        douyinuserRepository.Update(douyinuser);
                    }
                }
                else
                {
                    var douyinuser = input.CopyTo<EditDouyinUserReq, DouyinUser>();
                    douyinuserRepository.Add(douyinuser);
                }
            });
            return ApiResult.Ok(true);
        }

        public async Task<ApiResult<PageQueryResonseBase<GetDouyinUserResp>>> GetDouyinUserByPage(PageQueryInputBase input)
        {
            var page = await douyinuserRepository.GetManyByPageAsync(x => true, input.GetSkip(), input.PageSize);
            var response = new PageQueryResonseBase<GetDouyinUserResp>(page.lists.Select(x => x.CopyTo<DouyinUser, GetDouyinUserResp>()).ToList(), page.total);
            return ApiResult<PageQueryResonseBase<GetDouyinUserResp>>.Ok(response);
        }

        public async Task<ApiResult> DeleteDouyinUser(DeleteModelReq input)
        {
            if (input != null && input.IdLists != null && input.IdLists.Any())
            {
                await unitofWork.ExecuteTransaction(() => {
                  douyinuserRepository.Delete(x => input.IdLists.Contains(x.Id));
                });
            }
            return ApiResult.Ok(true);
        }
    }
}