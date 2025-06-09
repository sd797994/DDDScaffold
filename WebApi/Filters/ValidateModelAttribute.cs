using InfrastructureBase;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(kv => kv.Value.Errors.Count > 0)
                    .Select(x=>x.Value.Errors.FirstOrDefault().ErrorMessage).FirstOrDefault();
                throw new ApplicationServiceException(errors);
            }
        }
    }
}
