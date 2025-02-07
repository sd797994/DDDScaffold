using Infrastructure;

namespace WebApi.Middleware
{
    public class LocalJobScheduleService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        public LocalJobScheduleService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //本地简单作业
                //LocalJobManager.RegisterJob<IVideoPubTaskApplicationService>(nameof(IVideoPubTaskApplicationService.CancelProcessTask), 60 * 5);
                LocalJobManager.Start(serviceProvider);
            }
            catch (Exception)
            {
                LocalJobManager.Stop();
            }
            return Task.CompletedTask;
        }
    }
}
