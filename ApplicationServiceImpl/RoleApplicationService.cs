using ApplicaionServiceInterface.Dtos.Bases;
using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Interface;
using Domain.Entities;
using Domain.IRespoitory;
using Infrastructure.DataBase;
using Infrastructure.EfDataAccess;
using InfrastructureBase;
using InfrastructureBase.Object;
using Microsoft.EntityFrameworkCore;

namespace ApplicationService.ApplicationServiceImpl
{
    public class RoleApplicationService : IRoleApplicationService
    {
        private readonly IUserRepository userRepository;
        private readonly IRoleRepository roleRepository;
        private readonly IUnitofWork unitofWork;
        private readonly IPermissionApplicationService permissionApplicationService;
        private MySqlEfContext mySqlEfContext;
        public RoleApplicationService(IRoleRepository roleRepository, IUnitofWork unitofWork, IPermissionApplicationService permissionApplicationService, MySqlEfContext mySqlEfContext, IUserRepository userRepository)
        {
            this.roleRepository = roleRepository;
            this.unitofWork = unitofWork;
            this.permissionApplicationService = permissionApplicationService;
            this.mySqlEfContext = mySqlEfContext;
            this.userRepository = userRepository;
        }
        public async Task SaveRole(EditRoleReqVo input)
        {
            if (input != null)
            {
                await unitofWork.ExecuteTransaction(async () =>
                {
                    var needadd = true;
                    if (input.Id != null && input.Id != 0)
                    {
                        var role = await roleRepository.GetAsync(input.Id.Value);
                        if (role != null)
                        {
                            needadd = false;
                            role.Name = input.Name;
                            role.Status = input.Status;
                            role.RoleType = input.RoleType;
                            roleRepository.Update(role);
                            await mySqlEfContext.RolePermission.Where(x => x.RoleId == role.Id).ExecuteDeleteAsync();
                            if (input.RoleType == Domain.Enums.UserRoleType.Nom)
                                input.MenuIds.ForEach(x => mySqlEfContext.RolePermission.Add(new Infrastructure.DataBase.PO.RolePermission() { RoleId = role.Id, PermissionId = x }));
                        }
                    }
                    if (needadd)
                    {
                        var role = new Role();
                        role.Name = input.Name;
                        role.Status = 1;
                        role.RoleType = input.RoleType;
                        var oprole = roleRepository.Add(role);
                        await unitofWork.CommitAsync();
                        if (input.RoleType == Domain.Enums.UserRoleType.Nom)
                            input.MenuIds.ForEach(x => mySqlEfContext.RolePermission.Add(new Infrastructure.DataBase.PO.RolePermission() { RoleId = oprole.Id, PermissionId = x }));
                    }
                });
                return;
            }
            throw new ApplicationServiceException("没有传递有效的数据，无法进行记录增加/更新");
        }

        public async Task DeleteRole(DeleteModelReq input)
        {
            if (input != null && input.IdLists != null && input.IdLists.Any())
            {
                await unitofWork.ExecuteTransaction(async () =>
                {
                    roleRepository.Delete(x => input.IdLists.Contains(x.Id));
                    await mySqlEfContext.RolePermission.Where(x => input.IdLists.Contains(x.RoleId)).ExecuteDeleteAsync();
                });
            }
        }
        public async Task<PageQueryResonseBase<RoleListReqVo>> GetAllRoleByPage(PageQueryInputBase input)
        {
            var rolesPage = await roleRepository.GetManyByPageAsync(x => true, input.GetSkip(), input.PageSize);
            var roleids = rolesPage.lists.Select(x => x.Id);
            var lastuserid = rolesPage.lists.Where(x => x.LastUpdateUserId != null && x.LastUpdateUserId > 0).Select(x => x.LastUpdateUserId).Distinct().ToList();
            var rolepermessions = await mySqlEfContext.RolePermission.Where(x => roleids.Contains(x.RoleId)).ToListAsync();
            var resultlist = new List<RoleListReqVo>();
            var allpermession = await permissionApplicationService.GetAllPermission(null);
            var optuser = await userRepository.GetManyAsync(x => lastuserid.Contains(x.Id));
            rolesPage.lists.ForEach(x =>
            {
                var item = x.CopyTo<RoleListReqVo>();
                item.LastUpdateUserName = optuser.FirstOrDefault(y => y.Id == x.LastUpdateUserId)?.RealName;
                if (item.RoleType == Domain.Enums.UserRoleType.Sup)
                {
                    item.Menus = allpermession;
                }
                else if (rolepermessions.Any())
                {
                    item.Menus = rolepermessions.Where(y => y.RoleId == x.Id).Select(x => allpermession.FirstOrDefault(y => y.Id == x.PermissionId)).ToList();
                }
                resultlist.Add(item);
            });
            var result = new PageQueryResonseBase<RoleListReqVo>(resultlist, rolesPage.total);
            return result;
        }
        public async Task<List<RoleListReqVo>> GetAllRole()
        {
            var roles = await roleRepository.GetManyAsync(x => true);
            var roleids = roles.Select(x => x.Id);
            var rolepermessions = await mySqlEfContext.RolePermission.Where(x => roleids.Contains(x.RoleId)).ToListAsync();
            var allpermession = await permissionApplicationService.GetAllPermission(null);
            var resultlist = new List<RoleListReqVo>();
            roles.ForEach(x =>
            {
                var item = new RoleListReqVo();
                item.Id = x.Id;
                item.Name = x.Name;
                item.Status = x.Status;
                if (item.RoleType == Domain.Enums.UserRoleType.Sup)
                {
                    item.Menus = allpermession;
                }
                else if (rolepermessions.Any())
                {
                    item.Menus = rolepermessions.Where(y => y.RoleId == x.Id).Select(x => allpermession.FirstOrDefault(y => y.Id == x.PermissionId)).ToList();
                }
                resultlist.Add(item);
            });
            return resultlist;
        }
        public async Task BatchStatusRole(IdListStatusReqVo input)
        {
            if (input != null && input.IdLists != null && input.IdLists.Any())
            {
                await unitofWork.ExecuteTransaction(() =>
                {
                    roleRepository.Update(x => input.IdLists.Contains(x.Id), x => x.Status, input.Status);
                });
            }
        }
    }
}