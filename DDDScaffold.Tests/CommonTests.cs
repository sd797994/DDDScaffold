using Infrastructure;
using ApplicaionServiceInterface.Dtos.Responses;
using Domain.Enums;
using Xunit;
using System.Collections.Generic;

namespace DDDScaffold.Tests;

public class CommonTests
{
    [Fact]
    public void GetMD5SaltCode_ReturnsSameResult_WithSameSalt()
    {
        var first = Common.GetMD5SaltCode("password", 123);
        var second = Common.GetMD5SaltCode("password", 123);
        Assert.Equal(first, second);
    }

    [Fact]
    public void GetMd5Hash_ReturnsExpectedHash()
    {
        var hash = Common.GetMd5Hash("hello");
        Assert.Equal("5d41402abc4b2a76b9719d911017c592", hash);
    }

    [Fact]
    public void BuildTree_FiltersNodesById()
    {
        var root = new MenuRespVo
        {
            Id = 1,
            MenuName = "Root",
            MenuType = PermissionMenuType.Type,
            Children = new List<MenuRespVo>
            {
                new MenuRespVo
                {
                    Id = 2,
                    MenuName = "Child1",
                    MenuType = PermissionMenuType.MenuList,
                    Children = new List<MenuRespVo>
                    {
                        new MenuRespVo { Id = 4, MenuName = "Grandchild", MenuType = PermissionMenuType.Button }
                    }
                },
                new MenuRespVo { Id = 3, MenuName = "Child2", MenuType = PermissionMenuType.MenuList }
            }
        };

        var result = Common.BuildTree(root, new HashSet<int> { 4 });

        Assert.NotNull(result);
        Assert.Single(result.Children); // only child1 should remain
        var child1 = result.Children[0];
        Assert.Equal(2, child1.Id);
        Assert.Single(child1.Children);
        Assert.Equal(4, child1.Children[0].Id);
    }
}
