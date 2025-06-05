using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Interface;
using Domain.Enums;
using Domain.Entities;
using ApplicaionServiceInterface.Dtos.Responses;
using ApplicaionServiceInterface.Dtos.Bases;
using Infrastructure;
using Infrastructure.Repository;
using Infrastructure.DataBase;
using Infrastructure.EfDataAccess;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DDDScaffold.Tests;

public class RoleApplicationServiceTests
{
    private class FakeUnitOfWork : IUnitofWork
    {
        private readonly DbContext _context;
        public FakeUnitOfWork(DbContext context) => _context = context;
        public Task CommitAsync() => _context.SaveChangesAsync();
        public async Task ExecuteTransaction(Func<Task> func)
        {
            await func();
            await _context.SaveChangesAsync();
        }
        public Task ExecuteTransaction(Action dbFunc)
        {
            dbFunc();
            return _context.SaveChangesAsync();
        }
    }
    private class DummyPermissionService : IPermissionApplicationService
    {
        public Task SavePermission(EditPermissionReq input) => Task.CompletedTask;
        public Task DeletePermission(DeleteModelReq input) => Task.CompletedTask;
        public Task<List<MenuRespVo>> GetAllPermission(MenuReqVo input) => Task.FromResult(new List<MenuRespVo>());
        public Task<PageQueryResonseBase<MenuRespVo>> GetPermissionByPage(PageQueryInputBase input) =>
            Task.FromResult(new PageQueryResonseBase<MenuRespVo>(new List<MenuRespVo>(), 0));
    }

    [Fact]
    public async Task SaveRole_AddsRoleAndPermissions()
    {
        Common.SetCurrentUser(new User { Id = 1, UserName = "admin" });
        var options = new DbContextOptionsBuilder<MySqlEfContext>()
            .UseInMemoryDatabase("role_db")
            .Options;
        using var context = new MySqlEfContext(options);
        var roleRepo = new RoleRepository(context);
        var userRepo = new UserRepository(context);
        var uow = new FakeUnitOfWork(context);
        var service = new ApplicationService.ApplicationServiceImpl.RoleApplicationService(roleRepo, uow, new DummyPermissionService(), context, userRepo);

        var req = new EditRoleReqVo
        {
            Name = "Admin",
            RoleType = UserRoleType.Nom,
            Status = 1,
            MenuIds = new List<int> { 1, 2 }
        };

        await service.SaveRole(req);

        var role = await context.Role.FirstOrDefaultAsync(r => r.Name == "Admin");
        Assert.NotNull(role);
        var links = await context.RolePermission.Where(rp => rp.RoleId == role.Id).ToListAsync();
        Assert.Equal(2, links.Count);
    }
}
