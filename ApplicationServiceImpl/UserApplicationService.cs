using ApplicaionServiceInterface.Dtos.Bases;
using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Dtos.Responses;
using ApplicaionServiceInterface.Interface;
using Domain.Entities;
using Domain.IRespoitory;
using Infrastructure;
using Infrastructure.DataBase;
using Infrastructure.EfDataAccess;
using InfrastructureBase;
using InfrastructureBase.Object;
using Microsoft.EntityFrameworkCore;

namespace ApplicationService.ApplicationServiceImpl
{
    public class UserApplicationService : IUserApplicationService
    {
        private readonly IUserRepository userRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IUnitofWork unitofWork;
        private readonly MySqlEfContext mySqlEfContext;
        private readonly IPermissionApplicationService permissionApplicationService;
        public UserApplicationService(IUserRepository userRepository, IUnitofWork unitofWork, MySqlEfContext mySqlEfContext, IPermissionApplicationService permissionApplicationService, IRoleRepository roleRepository)
        {
            this.userRepository = userRepository;
            this.unitofWork = unitofWork;
            this.mySqlEfContext = mySqlEfContext;
            this.permissionApplicationService = permissionApplicationService;
            this.roleRepository = roleRepository;
        }
        public async Task<ApiResult<LoginResp>> GetUserInfo(LoginReq input)
        {
            var user = await userRepository.GetAsync(x => x.UserName == input.UserName && x.Password == Common.GetMD5SaltCode(input.Password) && x.Status == 1);
            if (user == null)
                throw new ApplicationServiceException("账号不存在或已被停用");
            if (user.RoleType == Domain.Enums.UserRoleType.Nom)
            {
                var userroleCount = await (from a in mySqlEfContext.UserRole join b in mySqlEfContext.Role on a.RoleId equals b.Id where b.IsDelete == false && b.Status == 1 select a.Id).CountAsync();
                if (userroleCount == 0)
                    throw new ApplicationServiceException("账号角色已被停用");
            }
            var resp = user.CopyTo<User, LoginResp>();
            resp.Token = JwtService.GenerateToken(user.Id, user.UserName);
            resp.UserRoles = await mySqlEfContext.UserRole.Where(x => x.UserId == resp.Id).Select(x => x.RoleId).ToListAsync();
            return ApiResult<LoginResp>.Ok(resp);
        }

        public async Task<ApiResult> SaveUserInfo(EditUserReq input)
        {
            if (input != null)
            {
                if (input.Id == 0)
                {
                    input.Id = Common.GetCurrentUser().Id;//传0修改自己
                }
                await unitofWork.ExecuteTransactionAsync(async () =>
                {
                    User user;
                    if (input.Id != null && input.Id != 0)
                    {
                        user = await userRepository.GetAsync(input.Id.Value);
                        if (user != null)
                        {
                            user.UserName = input.UserName;
                            user.RealName = input.RealName;
                            user.Password = Common.GetMD5SaltCode(input.Password);
                            user.RoleType = input.RoleType;
                            user.Status = input.Status;
                            userRepository.Update(user);
                            if (input.RoleIdList != null)
                            {
                                await mySqlEfContext.UserRole.Where(x => x.UserId == user.Id).ExecuteDeleteAsync();
                                //更新用户角色
                                mySqlEfContext.UserRole.AddRange(input.RoleIdList.Select(x => new Infrastructure.DataBase.PO.UserRole() { UserId = user.Id, RoleId = x }));
                            }
                        }
                    }
                    else
                    {
                        user = input.CopyTo<EditUserReq, User>();
                        user.Password = Common.GetMD5SaltCode(input.Password);
                        var pouser = userRepository.Add(user);
                        await unitofWork.CommitAsync();
                        user.Id = pouser.Id;
                        if (input.RoleIdList != null)
                        {
                            mySqlEfContext.UserRole.AddRange(input.RoleIdList.Select(x => new Infrastructure.DataBase.PO.UserRole() { UserId = user.Id, RoleId = x }));
                        }
                    }
                });
                return await Task.FromResult(ApiResult.Ok(true));
            }
            throw new ApplicationServiceException("没有传递有效的数据，无法进行记录增加/更新");
        }

        public async Task<ApiResult<GetUserResp>> UserInfo(GetModelReq input)
        {
            if (input != null)
            {
                if (input.Id == 0)
                {
                    input.Id = Common.GetCurrentUser().Id;//传0查自身
                }
                var user = await userRepository.GetAsync(input.Id);
                if (user != null)
                {
                    var userresp = user.CopyTo<User, GetUserResp>();
                    return ApiResult<GetUserResp>.Ok(userresp);
                }
            }
            throw new ApplicationServiceException("没有查到对应的用户");
        }

        public async Task<ApiResult> BatchStatusRole(IdListStatusReqVo input)
        {
            if (input != null && input.IdLists != null && input.IdLists.Any())
            {
                await unitofWork.ExecuteTransaction(() =>
                {
                    userRepository.Update(x => input.IdLists.Contains(x.Id), x => x.Status, input.Status);
                });
            }
            return ApiResult.Ok(true);
        }
        public async Task<ApiResult<PageQueryResonseBase<GetUserResp>>> GetAllUserByPage(PageQueryInputBase input)
        {
            var rolesPage = await userRepository.GetManyByPageAsync(x => true, input.GetSkip(), input.PageSize);
            var alluserid = rolesPage.lists.Select(x => x.Id).ToList();
            var lastuserid = rolesPage.lists.Where(x => x.LastUpdateUserId != null && x.LastUpdateUserId > 0).Select(x => x.LastUpdateUserId).Distinct().ToList();
            var userrolelist = await (from userrole in mySqlEfContext.UserRole.Where(x => alluserid.Contains(x.UserId)) join role in mySqlEfContext.Role on userrole.RoleId equals role.Id select new { userrole.UserId, role.Id, role.Name }).ToListAsync();
            var userlist = new List<GetUserResp>();
            var optuser = await userRepository.GetManyAsync(x => lastuserid.Contains(x.Id));
            rolesPage.lists.ForEach(x =>
            {
                var item = x.CopyTo<User, GetUserResp>();
                item.LastUpdateUserName = optuser.FirstOrDefault(y => y.Id == x.LastUpdateUserId)?.RealName;
                item.RoleNameList = userrolelist.Where(y => y.UserId == x.Id).Select(y => new UserRoleResp() { id = y.Id, Name = y.Name }).ToList();
                userlist.Add(item);
            });
            var result = new PageQueryResonseBase<GetUserResp>(userlist, rolesPage.total);
            return ApiResult<PageQueryResonseBase<GetUserResp>>.Ok(result);
        }

        public async Task<ApiResult<List<MenuRespVo>>> GetRoleMenus(GetModelReq input)
        {
            if (input.Id == 0)
            {
                input.Id = Common.GetCurrentUser().Id;//传0修改自己
            }
            var user = await userRepository.GetAsync(input.Id);
            var all = (await permissionApplicationService.GetAllPermission(new MenuReqVo() { FlatMenu = false, ShowSystem = true })).Data;
            if (user.RoleType == Domain.Enums.UserRoleType.Sup)
                return ApiResult<List<MenuRespVo>>.Ok(all);
            var allRoles = await mySqlEfContext.UserRole.Where(x => x.UserId == input.Id).Select(x => x.RoleId).ToListAsync();
            var roles = await roleRepository.GetManyAsync(x => allRoles.Contains(x.Id));
            if (roles.Any())
            {
                if (roles.Any(x => x.RoleType == Domain.Enums.UserRoleType.Sup))
                {
                    return ApiResult<List<MenuRespVo>>.Ok(all);
                }
                else
                {
                    var roleids = roles.Select(x => x.Id).ToList();
                    var rolepermessions = await mySqlEfContext.RolePermission.Where(x => roleids.Contains(x.RoleId)).Select(x => x.PermissionId).ToListAsync();
                    var finalmenus = Common.BuildTree(new MenuRespVo() { Id = 0, Children = all }, rolepermessions.ToHashSet());
                    return ApiResult<List<MenuRespVo>>.Ok(finalmenus.Children.ToList());
                }
            }
            else
            {
                return ApiResult<List<MenuRespVo>>.Ok(new List<MenuRespVo>());
            }
        }

        public async Task<ApiResult> DeleteUser(DeleteModelReq input)
        {
            if (input != null && input.IdLists != null && input.IdLists.Any())
            {
                await unitofWork.ExecuteTransaction(() =>
                {
                    userRepository.Delete(x => input.IdLists.Contains(x.Id));
                });
            }
            return ApiResult.Ok(true);
        }
    }
}