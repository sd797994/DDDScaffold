using Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Middleware
{
    public class InitDatabaseCustomerService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        public InitDatabaseCustomerService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();
            using var dbcontext = scope.ServiceProvider.GetService<MySqlEfContext>();
            await dbcontext.Database.MigrateAsync();
            await Task.CompletedTask;
        }
    }
}
