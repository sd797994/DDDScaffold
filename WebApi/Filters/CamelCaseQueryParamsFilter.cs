using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace WebApi.Filters
{
    public class CamelCaseQueryParamsFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var parameter in operation.Parameters.Where(p => p.In == ParameterLocation.Query))
            {
                parameter.Name = JsonNamingPolicy.CamelCase.ConvertName(parameter.Name);
            }
        }
    }
}
