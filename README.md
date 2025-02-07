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

## 快速开始

1. **克隆或使用 NuGet 引用**  
   - 若你想直接克隆本仓库：
     ```bash
     git clone https://github.com/sd797994/DDDScaffold.git
     cd DDDScaffold
     ```
   
2. **添加引用**  
   - 在你的 **.NET 解决方案** 中，参考/引用对应的项目或包：
     - `ApplicationServiceInterface`（netstandard2.0），主要放接口定义。
     - `ApplicationServiceImpl`（net9.0或你的目标 .NET 版本），主要放接口实现。

3. **定义领域模型与应用服务**  
   - 在 **Domain** 层中创建你的 **领域实体 (Entities)**、**枚举 (Enums)** 和 **仓储接口 (IRepository)** 等。
   - 在 `ApplicationServiceInterface` 项目里，定义 **IApplicationService** 系列接口（对外暴露的方法契约）。
   - 在 `ApplicationServiceImpl` 项目里，实现这些接口，编写具体的业务逻辑。

4. **自动生成 Controller**  
   - Source Generator 会扫描 `ApplicationServiceInterface` 中的接口，自动生成相应的 Controller 代码。
   - 默认情况下，这些自动生成的 Controller 会放到指定的命名空间/文件夹（可在 `.csproj` 或生成器配置中设置）。
   - 你几乎无需编写手动的 `Controller`，让你把精力专注于应用逻辑。

5. **运行并测试**  
   - 进入 `WebApi` 项目目录后启动：
     ```bash
     dotnet run --project src/WebApi/WebApi.csproj
     ```
   - 你可以使用 [Swagger](https://swagger.io/tools/swagger-ui/)、[Postman](https://www.postman.com/) 等工具测试自动生成的接口。  
   - 你会发现，接口对应 `IApplicationService` 中的方法会自动暴露成 API 端点。

## 项目结构

通常一个示例解决方案结构可能如下所示：
├─ src │ ├─ Domain │ │ ├─ Entities │ │ ├─ Enums │ │ └─ IRepository │ ├─ ApplicationServiceInterface (netstandard2.0) │ │ ├─ IApplicationService1.cs │ │ └─ IApplicationService2.cs │ ├─ ApplicationServiceImpl (net9.0) │ │ ├─ ApplicationService1.cs │ │ └─ ApplicationService2.cs │ ├─ Infrastructure │ │ ├─ Repository │ │ └─ Migrations │ └─ WebApi │ ├─ Controllers (由 source generator 自动生成) │ ├─ Program.cs │ ├─ Startup.cs │ └─ ... └─ tests ├─ UnitTests └─ IntegrationTests

## 配置与定制

- **Source Generator 配置**  
  在 `ApplicationServiceInterface` 或 `WebApi` 的 `.csproj` 文件中，配置生成器选项，指定命名空间、输出路径等。

- **异常处理 / 日志**  
  在 `WebApi` 层可以集成 Serilog、NLog 或其他日志库，并通过中间件进行全局异常捕获，返回统一格式的错误。

- **验证**  
  你可使用 **FluentValidation** 或 .NET 内置 **DataAnnotation** 进行参数验证。只要在 Service 层或实体上声明验证规则，自动生成的 Controller 也能尊重这些验证逻辑。


## 许可证

本项目基于 **[MIT License](./LICENSE)** 开源，你可以自由地在商业或个人项目中使用。

## 联系与支持

- **问题反馈 / Bug 提交**：欢迎到 [Issues](https://github.com/sd797994/DDDScaffold/issues) 区提交。  
- **讨论 / 建议**：可以在 [Discussions](https://github.com/sd797994/DDDScaffold/discussions) 区提出想法或直接与维护者联系。  

如果你觉得本项目对你有所帮助，欢迎 **Star** 一下，非常感谢！

