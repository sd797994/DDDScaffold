using ApplicaionServiceInterface.Dtos.Requests;
using ApplicaionServiceInterface.Interface;
using Domain.Enums;
using Domain.Entities;
using Infrastructure;
using Infrastructure.Repository;
using Infrastructure.DataBase;
using Infrastructure.EfDataAccess;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace DDDScaffold.Tests;

public class PermissionApplicationServiceTests
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
    [Fact]
    public async Task SavePermission_AddsRecord()
    {
        Common.SetCurrentUser(new User { Id = 1, UserName = "admin" });
        var options = new DbContextOptionsBuilder<MySqlEfContext>()
            .UseInMemoryDatabase("perm_db")
            .Options;
        using var context = new MySqlEfContext(options);
        var repo = new PermissionRepository(context);
        var uow = new FakeUnitOfWork(context);
        var service = new ApplicationServiceImpl.PermissionApplicationService(repo, uow);

        var req = new EditPermissionReq
        {
            MenuName = "TestMenu",
            Pid = 0,
            MenuPage = "test",
            MenuType = PermissionMenuType.MenuList,
            MenuIcon = "icon",
            Status = PermissionStatus.Expand,
            Sort = 1,
            ShowSystem = false
        };

        await service.SavePermission(req);

        var saved = await context.Permission.FirstOrDefaultAsync(p => p.MenuName == "TestMenu");
        Assert.NotNull(saved);
    }
}
