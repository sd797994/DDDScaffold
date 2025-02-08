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
    public class PermissionApplicationService : IPermissionApplicationService
    {
        private readonly IPermissionRepository permissionRepository;
        private readonly IUnitofWork unitofWork;
        public PermissionApplicationService(IPermissionRepository permissionRepository, IUnitofWork unitofWork)
        {
            this.permissionRepository = permissionRepository;
            this.unitofWork = unitofWork;
        }

        public async Task<ApiResult> SavePermission(EditPermissionReq input)
        {
            if (input != null)
            {
                await unitofWork.ExecuteTransactionAsync(async () =>
                {
                    if (input.Id != 0)
                    {
                        var permission = await permissionRepository.GetAsync(input.Id);
                        if (permission != null)
                        {
                            permission.MenuName = input.MenuName;
                            permission.Pid = input.Pid;
                            permission.Status = input.Status;
                            permission.MenuType = input.MenuType;
                            permission.MenuPage = input.MenuPage;
                            permission.MenuIcon = input.MenuIcon;
                            permission.Sort = input.Sort;
                            permission.MenuDisplayInfo = input.MenuDisplayInfo;
                            permission.ShowSystem = input.ShowSystem;
                            permissionRepository.Update(permission);
                        }
                    }
                    else
                    {
                        var permission = input.CopyTo<Permission>();
                        permissionRepository.Add(permission);
                    }
                });
                return ApiResult.Ok(true);
            }
            throw new ApplicationServiceException("没有传递有效的数据，无法进行记录增加/更新");
        }
        public async Task<ApiResult> DeletePermission(DeleteModelReq input)
        {
            if (input != null && input.IdLists != null && input.IdLists.Any())
            {
                await unitofWork.ExecuteTransaction(() =>
                {
                    permissionRepository.Delete(x => input.IdLists.Contains(x.Id));
                });
            }
            return ApiResult.Ok(true);
        }
        public async Task<ApiResult<List<MenuRespVo>>> GetAllPermission(MenuReqVo input)
        {
            var result = new List<MenuRespVo>();
            var resultvo = new List<MenuRespVo>();
            Queue<MenuRespVo> queue = new Queue<MenuRespVo>();
            HashSet<MenuRespVo> hash = new HashSet<MenuRespVo>();
            if (input == null)
                input = new MenuReqVo() { ShowSystem = true, FlatMenu = true };
            foreach (var item in await permissionRepository.GetManyAsync(x => (input.ShowSystem == false ? x.ShowSystem == false : true)))
            {
                if (input.Filters == null || input.Filters.Contains(item.Id))
                    result.Add(item.CopyTo<MenuRespVo>());
            }
            //广度优先搜索
            //首先获取所有父级，写入哈希和队列以及结果集
            foreach (var item in result.Where(x => x.Pid == null || x.Pid == 0))
            {
                if (hash.Add(item))
                {
                    queue.Enqueue(item);
                    resultvo.Add(item);
                }
            }
            while (queue.Count > 0)
            {
                //从当前队列取出根，然后查找子
                var item = queue.Dequeue();
                //通过hash来避免重复入队
                var allChilds = result.Where(x => x.Pid == item.Id && !hash.Contains(x)).ToList();
                allChilds.ForEach(x =>
                {
                    x.Pid = item.Id;
                    x.Pname = item.MenuName;
                    var newPathList = new List<int>(item.PathList ?? new List<int>())
                    {
                        item.Id
                    };
                    x.PathList = newPathList;
                    hash.Add(x);
                    queue.Enqueue(x);
                    if (input.FlatMenu)
                    {
                        resultvo.Add(x);
                    }
                });
                item.Children = allChilds.OrderByDescending(x => x.Sort).ToList();
            }
            return ApiResult.Ok(resultvo.OrderByDescending(x => x.Sort).ToList());
        }
        public async Task<ApiResult<PageQueryResonseBase<MenuRespVo>>> GetPermissionByPage(PageQueryInputBase input)
        {
            var all = (await GetAllPermission(null)).Data;
            var page = all.Skip(input.GetSkip()).Take(input.PageSize).ToList();
            return ApiResult.Ok(new PageQueryResonseBase<MenuRespVo>(page, all.Count));
        }
    }
}