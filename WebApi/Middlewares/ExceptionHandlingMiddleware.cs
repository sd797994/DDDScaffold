using ApplicaionServiceInterface.Dtos.Bases;
using InfrastructureBase;
using InfrastructureBase.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace WebApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                if (context.Response.StatusCode == 401)
                {
                    await HandleExceptionAsync(context, null, "授权验证失败,请稍后再重新登录");
                }
            }
            catch (ApplicationServiceException ex)
            {
                await HandleExceptionAsync(context, ex, ex.Message);
            }
            catch (InfrastructureException ex)
            {
                await HandleExceptionAsync(context, ex, ex.Message);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("See the inner exception for details") && ex.InnerException != null)
                {
                    Console.WriteLine($"捕获到未处理异常，异常信息：{ex.InnerException.Message}\r\n异常堆栈：{ex.InnerException.StackTrace}");
                }
                else
                {
                    Console.WriteLine($"捕获到未处理异常，异常信息：{ex.Message}\r\n异常堆栈：{ex.StackTrace}");
                }
                await HandleExceptionAsync(context, ex, "出错了，请稍后再试");
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, string message = null)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            var result = JsonConvert.SerializeObject(ApiResult.Err(message), new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            });
            return context.Response.WriteAsync(result);
        }

    }
}
