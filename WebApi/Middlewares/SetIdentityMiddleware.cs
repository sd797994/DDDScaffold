using Infrastructure;
using System.Security.Claims;

namespace WebApi.Middleware
{
    public class SetIdentityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public SetIdentityMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = Convert.ToInt32(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var userName = context.User.FindFirst(ClaimTypes.Name)?.Value;
                Common.SetCurrentUser(new Domain.Entities.User() { Id = userId, UserName = userName });
            }
            await _next(context);
        }
    }
}
