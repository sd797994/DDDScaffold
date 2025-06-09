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

public class UserApplicationServiceTests
{
    private class DummyPermissionService : IPermissionApplicationService
    {
        public Task SavePermission(EditPermissionReq input) => Task.CompletedTask;
        public Task DeletePermission(DeleteModelReq input) => Task.CompletedTask;
        public Task<List<MenuRespVo>> GetAllPermission(MenuReqVo input) => Task.FromResult(new List<MenuRespVo>());
        public Task<PageQueryResonseBase<MenuRespVo>> GetPermissionByPage(PageQueryInputBase input) =>
            Task.FromResult(new PageQueryResonseBase<MenuRespVo>(new List<MenuRespVo>(), 0));
    }

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

    [Fact]
    public async Task GetUserInfo_ReturnsTokenAndRoles()
    {
        Common.SetCurrentUser(new User { Id = 1, UserName = "admin" });
        var options = new DbContextOptionsBuilder<MySqlEfContext>()
            .UseInMemoryDatabase("user_db")
            .Options;
        using var context = new MySqlEfContext(options);
        context.Role.Add(new Infrastructure.DataBase.PO.Role { Id = 1, Name = "Role1", Status = 1, RoleType = UserRoleType.Nom });
        context.User.Add(new Infrastructure.DataBase.PO.User
        {
            Id = 2,
            UserName = "test",
            RealName = "Test",
            Password = Common.GetMD5SaltCode("pass"),
            RoleType = UserRoleType.Nom,
            Status = 1
        });
        context.UserRole.Add(new Infrastructure.DataBase.PO.UserRole { UserId = 2, RoleId = 1 });
        await context.SaveChangesAsync();

        var userRepo = new UserRepository(context);
        var roleRepo = new RoleRepository(context);
        var uow = new FakeUnitOfWork(context);
        var service = new ApplicationService.ApplicationServiceImpl.UserApplicationService(userRepo, uow, context, new DummyPermissionService(), roleRepo);

        var resp = await service.GetUserInfo(new LoginReq { UserName = "test", Password = "pass" });

        Assert.Equal("test", resp.UserName);
        Assert.Single(resp.UserRoles);
        Assert.Contains(1, resp.UserRoles);
        Assert.False(string.IsNullOrEmpty(resp.Token));
    }
}
