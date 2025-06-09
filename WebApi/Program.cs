using Autofac.Extensions.DependencyInjection;
using Autofac;
using InfrastructureBase.Object;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using WebApi;
using Infrastructure.Object;
using Infrastructure.Data;
using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using WebApi.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using WebApi.Filters;

var builder = WebApplication.CreateBuilder(args);
if (false)//快速连接生产环境使用
{
    builder.Configuration.AddJsonFile("appsettings.Production.json", optional: false, reloadOnChange: true);
}
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ServiceModule());
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtService.NewSymmetricSecurityKey(),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("*") // 允许特定来源
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelAttribute>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
}).ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddDbContext<MySqlEfContext>(options => options.UseMySql(builder.Configuration.GetSection("SqlConnectionString").Value, new MySqlServerVersion(new Version(5, 7, 0))));
RedisDatabaseHelper.Initialize(builder.Configuration.GetSection("RedisConnectionString").Value);
builder.Services.AddHostedService<InitDatabaseCustomerService>();
builder.Services.AddHostedService<LocalJobScheduleService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 配置Bearer令牌
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token in the format: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    string[] xmls = ["WebApi", "ApplicaionServiceInterface", "Domain"];
    foreach (string xml in xmls)
        includexmlfile(xml);
    c.OperationFilter<CamelCaseQueryParamsFilter>();
    void includexmlfile(string filename)
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{filename}.xml");
        c.IncludeXmlComments(xmlPath, true);
    }
});
var app = builder.Build();
app.UseCors("AllowSpecificOrigin");
app.UseMiddleware<ExceptionHandlingMiddleware>(); 
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.InjectJavascript("/swaggerautologin.js"));
}
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SetIdentityMiddleware>();
app.MapControllers();
app.Run("http://*:80");
