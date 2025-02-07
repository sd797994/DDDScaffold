using DomainBase;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace WebApi.Controllers
{
    /// <summary>
    /// 代码生成专用控制器
    /// </summary>
    [ApiController]
    public class CodeBuilderController : ControllerBase
    {
        readonly Dictionary<Type, string> typeAliases = new Dictionary<Type, string>
        {
            { typeof(int), "int" },
            { typeof(short), "short" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(long), "long" },
            { typeof(bool), "bool" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(decimal), "decimal" },
            { typeof(string), "string" },
            { typeof(object), "object" },
            { typeof(char), "char" },
            { typeof(uint), "uint" },
            { typeof(ushort), "ushort" },
            { typeof(ulong), "ulong" },
            { typeof(void), "void" }
        };
        [HttpGet("/api/codebuilder")]
        public IActionResult CodeBuilder(string name, string desc)
        {
            //从domain获取所有的类型
            var entityType = typeof(Domain.Entities.User).Assembly.GetTypes().FirstOrDefault(x => x.BaseType == typeof(Entity) && x.Name.ToLower().Contains(name.ToLower()));
            if (entityType != null)
            {
                var code1 = """
                    using Domain.Entities;
                    using DomainBase;
                    namespace Domain.IRespoitory
                    {
                        public interface I_typename_Repository: IRepository<_typename_>
                        {

                        }
                    }
                    """;
                code1 = code1.Replace("_typename_", entityType.Name);
                var code2 = """
                    using Domain.IRespoitory;
                    using Infrastructure.DataBase;
                    using Infrastructure.DataBase.PO;
                    using Infrastructure.EfDataAccess;

                    namespace Infrastructure.Repository
                    {
                        public class _typename_Repository : RepositoryBase<MySqlEfContext, Domain.Entities._typename_, _typename_>, I_typename_Repository
                        {
                            public _typename_Repository(MySqlEfContext sqlEfContext) : base(sqlEfContext) { }
                        }
                    }
                    """;
                code2 = code2.Replace("_typename_", entityType.Name);
                var code3 = """
                    using ApplicaionServiceInterface.Dtos.Bases;
                    using ApplicaionServiceInterface.Dtos.Requests;
                    using ApplicaionServiceInterface.Dtos.Responses;
                    using ApplicaionServiceInterface.Interface.Attributes;
                    using System.Collections.Generic;
                    using System.Threading.Tasks;

                    namespace ApplicaionServiceInterface.Interface
                    {
                        [ActionGenerator("_desc_管理")]
                        public interface I_typename_ApplicationService
                        {
                            [ActionGeneratorMethod(RequestType.GET, "获取_desc_", "getinfo", true)]
                            Task<ApiResult<Get_typename_Resp>> Get_typename_Info(GetModelReq input);
                            [ActionGeneratorMethod(RequestType.Post,"保存_desc_", "edit", true)]
                            Task<ApiResult> Save_typename_(Edit_typename_Req input);
                            [ActionGeneratorMethod(RequestType.GET, "获取_desc_分页", "page", true)]
                            Task<ApiResult<PageQueryResonseBase<Get_typename_Resp>>> Get_typename_ByPage(PageQueryInputBase input);
                            [ActionGeneratorMethod(RequestType.Post, "删除_desc_", "delete", true)]
                            Task<ApiResult> Delete_typename_(DeleteModelReq input);
                        }
                    }
                    """;
                code3 = code3.Replace("_typename_", entityType.Name).Replace("_desc_", desc);
                var code4 = """
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
                        public class _typename_ApplicationService : I_typename_ApplicationService
                        {
                            private readonly I_typename_Repository _stypename_Repository;
                            private readonly IUnitofWork unitofWork;
                            public _typename_ApplicationService(I_typename_Repository _stypename_Repository, IUnitofWork unitofWork)
                            {
                                this._stypename_Repository = _stypename_Repository;
                                this.unitofWork = unitofWork;
                            }
                            public async Task<ApiResult<Get_typename_Resp>> Get_typename_Info(GetModelReq input)
                            {
                                if (input != null && input.Id != 0)
                                {
                                    var _stypename_ = await _stypename_Repository.GetAsync(input.Id);
                                    if (_stypename_ != null)
                                    {
                                        return ApiResult<Get_typename_Resp>.Ok(_stypename_.CopyTo<_typename_, Get_typename_Resp>());
                                    }
                                }
                                throw new ApplicationServiceException("没有找到对应的记录,请确定id是否正确");
                            }

                            public async Task<ApiResult> Save_typename_(Edit_typename_Req input)
                            {
                                if (input == null)
                                {
                                    throw new ApplicationServiceException("没有传递有效的数据，无法进行记录增加/更新");
                                }
                                await unitofWork.ExecuteTransactionAsync(async () => {
                                    if (input.Id != 0)
                                    {
                                        var _stypename_ = await _stypename_Repository.GetAsync(input.Id);
                                        if (_stypename_ != null)
                                        {
                                            //按照实际情况更新信息
                        _stypenameset_
                                            _stypename_Repository.Update(_stypename_);
                                        }
                                    }
                                    else
                                    {
                                        var _stypename_ = input.CopyTo<Edit_typename_Req, _typename_>();
                                        _stypename_Repository.Add(_stypename_);
                                    }
                                });
                                return ApiResult.Ok(true);
                            }

                            public async Task<ApiResult<PageQueryResonseBase<Get_typename_Resp>>> Get_typename_ByPage(PageQueryInputBase input)
                            {
                                var page = await _stypename_Repository.GetManyByPageAsync(x => true, input.GetSkip(), input.PageSize);
                                var response = new PageQueryResonseBase<Get_typename_Resp>(page.lists.Select(x => x.CopyTo<_typename_, Get_typename_Resp>()).ToList(), page.total);
                                return ApiResult<PageQueryResonseBase<Get_typename_Resp>>.Ok(response);
                            }

                            public async Task<ApiResult> Delete_typename_(DeleteModelReq input)
                            {
                                if (input != null && input.IdLists != null && input.IdLists.Any())
                                {
                                    await unitofWork.ExecuteTransaction(() => {
                                      _stypename_Repository.Delete(x => input.IdLists.Contains(x.Id));
                                    });
                                }
                                return ApiResult.Ok(true);
                            }
                        }
                    }
                    """;
                var setprops = Setprops(entityType);
                code4 = code4.Replace("_typename_", entityType.Name).Replace("_stypenameset_", setprops).Replace("_stypename_", entityType.Name.ToLower());
                var code5 = """
                    using Domain.Enums;
                    namespace ApplicaionServiceInterface.Dtos.Responses
                    {
                        /// <summary>
                        /// 获取_desc_
                        /// </summary>
                        public class Get_typename_Resp
                        {
                    _props_
                        }
                    }
                    """;
                var props = Getprops(entityType);
                code5 = code5.Replace("_typename_", entityType.Name).Replace("_props_", props).Replace("_desc_", desc);
                var code6 = """
                    using Domain.Enums;
                    namespace ApplicaionServiceInterface.Dtos.Requests
                    {
                    
                        /// <summary>
                        /// 编辑_desc_
                        /// </summary>
                        public class Edit_typename_Req
                        {
                    _props_
                        }
                    }
                    """;
                code6 = code6.Replace("_typename_", entityType.Name).Replace("_props_", props).Replace("_desc_", desc);
                var code7 = """
                    using System;
                    using System.Collections.Generic;
                    using System.Linq;
                    using System.Text;
                    using System.Threading.Tasks;

                    namespace Infrastructure.DataBase.PO
                    {
                        public class _typename_ : Domain.Entities._typename_
                        {

                        }
                    }
                    """;
                code7 = code7.Replace("_typename_", entityType.Name);
                // 创建内存流存储ZIP文件
                MemoryStream zipStream = new MemoryStream();

                using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    // 创建文件和文件夹结构
                    var folders = new string[]{"ApplicationServiceImpl", "ApplicaionServiceInterface/Interface",
                        "ApplicaionServiceInterface/Dtos/Requests","ApplicaionServiceInterface/Dtos/Responses","Domain/IRespoitory","Infrastructure/Repository","Infrastructure/DataBase/PO" };
                    CreateFile(Path.Combine(folders[4], $"I{entityType.Name}Repository.cs"), code1);
                    CreateFile(Path.Combine(folders[5], $"{entityType.Name}Repository.cs"), code2);
                    CreateFile(Path.Combine(folders[1], $"I{entityType.Name}ApplicationService.cs"), code3);
                    CreateFile(Path.Combine(folders[0], $"{entityType.Name}ApplicationService.cs"), code4);
                    CreateFile(Path.Combine(folders[3], $"Get{entityType.Name}Resp.cs"), code5);
                    CreateFile(Path.Combine(folders[2], $"Edit{entityType.Name}Req.cs"), code6);
                    CreateFile(Path.Combine(folders[6], $"{entityType.Name}.cs"), code7);
                    void CreateFile(string fullpath, string content)
                    {
                        ZipArchiveEntry fileEntry = zip.CreateEntry(fullpath);
                        using (StreamWriter writer = new StreamWriter(fileEntry.Open()))
                        {
                            writer.Write(content);
                        }
                    }
                }

                // 重置流的位置到开始
                zipStream.Position = 0;
                return File(zipStream, "application/zip", $"{name}_Download.zip");
            }
            string Getprops(Type entityType)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("        /// <summary>");
                sb.AppendLine("        /// id");
                sb.AppendLine("        /// </summary>");
                sb.AppendLine($"        public int Id {{ get; set; }}");
                foreach (var item in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (Attribute.IsDefined(item, typeof(NotMappedAttribute)))
                    {
                        continue;
                    }
                    Type underlyingType = Nullable.GetUnderlyingType(item.PropertyType);
                    string alias = "";
                    if (underlyingType != null)
                    {
                        if (typeAliases.TryGetValue(underlyingType, out string alias1))
                        {
                            alias = $"{alias1}?";
                        }
                        else
                        {
                            alias = $"{underlyingType.Name}?";
                        }
                    }
                    else
                    {
                        if (typeAliases.TryGetValue(item.PropertyType, out string alias1))
                        {
                            alias = alias1;
                        }
                        else
                        {
                            alias = item.PropertyType.Name;
                        }
                    }
                    var summary = GetPropertySummary(item);
                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine("        /// " + summary);
                    sb.AppendLine("        /// </summary>");
                    sb.AppendLine($"        public {alias} {item.Name} {{ get; set; }}");
                }
                return sb.ToString();
            }
            string GetPropertySummary(PropertyInfo property)
            {
                if (loadandreadxml("WebApi", out string value))
                {
                    return value;
                }
                else if (loadandreadxml("ApplicaionServiceInterface", out value))
                {
                    return value;
                }
                else if (loadandreadxml("Domain", out value))
                {
                    return value;
                }
                else if (loadandreadxml("Infrastructure", out value))
                {
                    return value;
                }
                return "";
                bool loadandreadxml(string location, out string value)
                {
                    value = "";
                    string assemblyPath = Assembly.GetExecutingAssembly().Location;
                    string xmlPath = Path.Combine(Path.GetDirectoryName(assemblyPath), $"{location}.xml");
                    var xdoc = XDocument.Load(xmlPath);
                    var memberName = $"P:{property.DeclaringType.FullName}.{property.Name}";
                    var summary = xdoc.Descendants("member")
                                      .FirstOrDefault(m => m.Attribute("name")?.Value == memberName)?
                                      .Descendants("summary")
                                      .FirstOrDefault();
                    if (summary == null)
                        return false;
                    else
                    {
                        value = summary?.Value.Replace("\n", "").Trim();
                        return true;
                    }
                }
            }
            string Setprops(Type entityType)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    sb.AppendLine($"                    _stypename_.{item.Name} = input.{item.Name};");
                }
                return sb.ToString();
            }
            throw new InfrastructureBase.ApplicationServiceException("未能找到实体");
        }
    }
}
