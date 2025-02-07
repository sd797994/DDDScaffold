using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

[Generator]
public class ActionGenerator : IIncrementalGenerator
{
    const string actionGeneratorAttributeName = "ApplicaionServiceInterface.Interface.Attributes.ActionGeneratorAttribute";
    const string actionMethodGeneratorAttributeName = "ApplicaionServiceInterface.Interface.Attributes.ActionGeneratorMethodAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //Debugger.Launch();
        // 获取编译对象
        IncrementalValueProvider<Compilation> compilationProvider = context.CompilationProvider;

        // 注册源代码输出
        context.RegisterSourceOutput(compilationProvider, (sourceProductionContext, compilation) =>
        {
            // 获取所有类型
            var allTypes = GetAllTypes(compilation);

            // 筛选具有 ActionGeneratorAttribute 的接口
            var interfacesWithAttribute = new List<INamedTypeSymbol>();

            foreach (var type in allTypes)
            {
                if (type.TypeKind == TypeKind.Interface && HasActionGeneratorAttribute(type, compilation))
                {
                    interfacesWithAttribute.Add(type);
                }
            }

            foreach (var interfaceType in interfacesWithAttribute)
            {
                GenerateCode(sourceProductionContext, interfaceType, compilation);
            }
        });
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation)
    {
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(compilation.GlobalNamespace);

        while (stack.Count > 0)
        {
            var namespaceSymbol = stack.Pop();

            foreach (var member in namespaceSymbol.GetMembers())
            {
                if (member is INamespaceSymbol childNamespace)
                {
                    stack.Push(childNamespace);
                }
                else if (member is INamedTypeSymbol typeSymbol)
                {
                    yield return typeSymbol;

                    foreach (var nestedType in GetNestedTypes(typeSymbol))
                    {
                        yield return nestedType;
                    }
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol typeSymbol)
    {
        foreach (var nestedType in typeSymbol.GetTypeMembers())
        {
            yield return nestedType;

            foreach (var childNestedType in GetNestedTypes(nestedType))
            {
                yield return childNestedType;
            }
        }
    }

    private static bool HasActionGeneratorAttribute(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var attributeSymbol = compilation.GetTypeByMetadataName(actionGeneratorAttributeName);

        if (attributeSymbol == null)
        {
            // 未找到属性类型
            return false;
        }

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeSymbol))
            {
                return true;
            }
        }

        return false;
    }
    private void GenerateCode(SourceProductionContext context, INamedTypeSymbol interfaceSymbol, Compilation compilation)
    {
        // 获取接口名称和描述
        var name = interfaceSymbol.Name.Replace("ApplicationService", "").Substring(1);
        var desc = "";

        // 获取属性数据
        var attributeSymbol = compilation.GetTypeByMetadataName(actionGeneratorAttributeName);
        var attributeData = interfaceSymbol.GetAttributes()
            .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(
                attr.AttributeClass, attributeSymbol));

        if (attributeData != null)
        {
            foreach (var namedArg in attributeData.ConstructorArguments)
            {
                desc = namedArg.Value as string;
                break;
            }
        }

        // 生成代码模板
        var code = $$"""
            using ApplicaionServiceInterface.Interface;
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Authorization;
            using InfrastructureBase.Object;
            using ApplicaionServiceInterface.Dtos.Bases;
            using ApplicaionServiceInterface.Dtos.Requests;
            using ApplicaionServiceInterface.Dtos.Responses;
            using System.Collections.Generic;
            namespace WebApi.Controllers
            {
                /// <summary>
                /// {{desc}}
                /// </summary>
                [ApiController]
                public class {{name}}Controller: ControllerBase
                {
                     private readonly {{interfaceSymbol.ToDisplayString(FullyQualifiedWithoutGlobalFormat)}} service;
                     public {{name}}Controller({{interfaceSymbol.ToDisplayString(FullyQualifiedWithoutGlobalFormat)}} service)
                     {
                         this.service = service;
                     }
            _methods_
                }
            }
            """;

        var methodsCode = new List<string>();

        // 获取方法属性类型，用于比较
        var methodAttributeSymbol = compilation.GetTypeByMetadataName(actionMethodGeneratorAttributeName);

        // 处理接口中的方法
        foreach (var member in interfaceSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
                continue;

            if (methodSymbol.MethodKind != MethodKind.Ordinary)
                continue;

            var methodAttr = methodSymbol.GetAttributes()
                .FirstOrDefault(attr => SymbolEqualityComparer.Default.Equals(
                    attr.AttributeClass, methodAttributeSymbol));

            if (methodAttr == null)
                continue;

            // 提取属性信息
            var methodDesc = "";
            var requestType = "";
            var authorizeCheck = false;
            var routeName = "";

            requestType = (int)methodAttr.ConstructorArguments[0].Value == 0 ? "GET" : "Post";
            methodDesc = methodAttr.ConstructorArguments[1].Value as string;
            routeName = methodAttr.ConstructorArguments[2].Value as string;
            authorizeCheck = (bool)methodAttr.ConstructorArguments[3].Value;

            var parameters = methodSymbol.Parameters;
            var paramType = parameters.Length > 0 ? parameters[0].Type.ToDisplayString(FullyQualifiedWithoutGlobalFormat) : "";
            var paramName = parameters.Length > 0 ? parameters[0].Name : "";
            var returnType = methodSymbol.ReturnType.ToDisplayString(FullyQualifiedWithoutGlobalFormat);

            var getpost = "";
            if (!string.IsNullOrEmpty(paramType))
            {
                if (requestType == "GET")
                    getpost = "[FromQuery]";
                else if (requestType == "Post")
                    getpost = "[FromBody]";
            }

            var reqTypeAttribute = requestType switch
            {
                "GET" => "[HttpGet]",
                "Post" => "[HttpPost]",
                _ => ""
            };

            if (authorizeCheck)
                reqTypeAttribute += "\n        [Authorize]";

            routeName = string.IsNullOrWhiteSpace(routeName) ? methodSymbol.Name.ToLower() : routeName;
            var paramLine = string.IsNullOrWhiteSpace(paramName)
    ? ""
    : $"\n        /// <param name=\"{paramName}\"></param>";
            var methodCode = $$"""
                    /// <summary>
                    /// {{methodDesc}}
                    /// </summary>{{paramLine}}
                    /// <returns></returns>
                    [Route("/api/{{name.ToLower()}}/{{routeName}}")]
                    {{reqTypeAttribute}}
                    public async {{returnType}} {{methodSymbol.Name}}({{(string.IsNullOrWhiteSpace(paramName) ? "" : $"{getpost}{paramType} {paramName}")}})
                    {
                        return await service.{{methodSymbol.Name}}({{paramName}});
                    }
            """;

            methodsCode.Add($"\n{methodCode}");
        }

        code = code.Replace("_methods_", string.Join("\r\n", methodsCode));

        context.AddSource($"{name}Controller.cs", SourceText.From(code, Encoding.UTF8));
    }
    private static readonly SymbolDisplayFormat FullyQualifiedWithoutGlobalFormat = new(
    globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);
}
