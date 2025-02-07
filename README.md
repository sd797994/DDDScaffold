# DDDScaffold

> 基于 **DDD** 思想、面向 **.NET** 的脚手架框架，通过 **自动生成Controller** 简化重复代码，提供最小化的控制器逻辑和清晰的多层架构。

## 功能特性

- **自动生成 Controller**  
  利用 Source Generator，根据 `ApplicationServiceInterface` 中定义的接口自动生成对应的 Controller，减少重复编写 CRUD 等样板代码。

- **DDD 分层架构**  
  提供对 `Domain`、`Infrastructure`、`Application`、`WebApi` 等分层的约定与实践，结构清晰、可维护。

- **最小化的控制器逻辑**  
  将业务逻辑、验证等集中在 Service 层，Controller 仅负责调用 Service 并返回结果。

- **统一异常处理与日志**  
  在 `WebApi` 层可集成全局异常捕获及日志功能（Serilog / NLog 等），实现统一的错误返回格式。

- **灵活的 .NET 版本支持**  
  `ApplicationServiceInterface` 使用 **netstandard2.0**，与自动代码生成器兼容性更好，而其实现层 `ApplicationServiceImpl` 可使用更高版本（如 **net9.0**），满足现代 .NET 新特性的需求。
  
- **模板代码生成能力**  
  除了Controller自动代码生成，还提供了模型的CRUD代码生成，在`Domain`的 **领域实体 (Entities)** 文件夹中创建好实体后，启动项目，访问`http://localhost/swagger/index.html`，会暴露一个业务代码生成接口，输入实体名称和描述即可自动生成应用层、仓储相关代码，可直接拷贝到项目中使用。

## 快速开始

1. **克隆使用**  
   - 若你想直接克隆本仓库：
     ```bash
     git clone https://github.com/sd797994/DDDScaffold.git
     cd DDDScaffold
     ```

2. **定义领域模型与应用服务**  
   - 在 **Domain** 层中创建你的 **领域实体 (Entities)**、**枚举 (Enums)** 和 **仓储接口 (IRepository)** 等。
   - 在 `ApplicationServiceInterface` 项目里，定义 **IApplicationService** 系列接口（对外暴露的方法契约）。
   - 在 `ApplicationServiceImpl` 项目里，实现这些接口，编写具体的业务逻辑。
   OR
   - 在 **Domain** 层中创建你的 **领域实体 (Entities)**、**枚举 (Enums)** 。
   - F5启动项目，访问`http://localhost/swagger/index.html` 使用/api/codebuilder接口自动生成包含CRUD的`ApplicationServiceInterface`、`ApplicationServiceImpl`、`IRepository`、`Repository`代码。
   
3. **自动生成 Controller**  
   - Source Generator 会扫描 `ApplicationServiceInterface` 中的接口，自动生成相应的 Controller 代码。
   - 默认情况下，这些自动生成的 Controller 会放到指定的命名空间/文件夹（可在 `.csproj` 或生成器配置中设置）。
   - 你几乎无需编写手动的 `Controller`，让你把精力专注于应用逻辑。

4. **运行并测试**  
   - 首次运行需要创建数据库并修改`appsettings.json`指向你的数据库，通过执行根目录下的`数据库迁移命令.bat`来初始化RBAC的数据库迁移命令
   - 进入 `WebApi` 项目目录后启动：
     ```bash
     dotnet run --project src/WebApi/WebApi.csproj
     ```
   - 你可以使用 [Swagger](https://swagger.io/tools/swagger-ui/)、[Postman](https://www.postman.com/) 等工具测试自动生成的接口。  
   - 你会发现，接口对应 `IApplicationService` 中的方法会自动暴露成 API 端点。

## 许可证

本项目基于 **[MIT License](./LICENSE)** 开源，你可以自由地在商业或个人项目中使用。

## 联系与支持

- **问题反馈 / Bug 提交**：欢迎到 [Issues](https://github.com/sd797994/DDDScaffold/issues) 区提交。  
- **讨论 / 建议**：可以在 [Discussions](https://github.com/sd797994/DDDScaffold/discussions) 区提出想法或直接与维护者联系。  

如果你觉得本项目对你有所帮助，欢迎 **Star** 一下，非常感谢！

