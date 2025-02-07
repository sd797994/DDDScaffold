using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

namespace Infrastructure
{
    public static class LocalJobManager
    {
        static List<JobContainer> jobContainers = new List<JobContainer>();
        public static void RegisterJob<T>(string method, int timeSecond) where T : class
        {
            if (timeSecond <= 0)
                throw new Exception("作业调度时间周期不能小于1秒");
            var serviceType = typeof(T);
            jobContainers.Add(new JobContainer()
            {
                ServiceType = serviceType,
                JobName = method,
                JobMethod = MethodBuilder(serviceType.GetMethod(method)),
                Second = timeSecond,
                ManualResetEvent = new ManualResetEvent(false),
                CancellationTokenSource = new CancellationTokenSource()
            });
        }
        static Func<object, Task> MethodBuilder(MethodInfo methodInfo)
        {
            ParameterExpression param = Expression.Parameter(typeof(object), "instance");
            UnaryExpression instanceCast = Expression.Convert(param, methodInfo.DeclaringType);
            MethodCallExpression methodCall = Expression.Call(instanceCast, methodInfo);
            Expression<Func<object, Task>> lambda = Expression.Lambda<Func<object, Task>>(methodCall, param);
            Func<object, Task> func = lambda.Compile();
            return func;
        }
        public static void TriggerJob<T>(string method) => jobContainers.FirstOrDefault(x => x.JobName == method && x.ServiceType == typeof(T))?.ManualResetEvent.Set();
        public static void Start(IServiceProvider serviceProvider)
        {
            foreach (var jobContainer in jobContainers)
            {
                jobContainer.TaskRunner = Task.Run(async () =>
                {
#if !DEBUG
                    await Task.Delay(60 * 1000);//等待1分钟，避免异常任务导致容器启动就死机
#endif
                    while (!jobContainer.CancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            using var scope = serviceProvider.CreateScope();
                            var service = scope.ServiceProvider.GetService(jobContainer.ServiceType);
                            await jobContainer.JobMethod(service);
                            if (jobContainer.ManualResetEvent.WaitOne(jobContainer.Second * 1000))
                            {
                                jobContainer.ManualResetEvent.Reset(); //重置事件状态
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }, jobContainer.CancellationTokenSource.Token);
            }
        }
        public static void Stop()
        {
            foreach (var jobContainer in jobContainers)
            {
                jobContainer.CancellationTokenSource.Cancel();
                jobContainer.ManualResetEvent.Dispose();
            }
        }
        class JobContainer
        {
            public string JobName { get; set; }
            public Type ServiceType { get; set; }
            public Func<object, Task> JobMethod { get; set; }
            public int Second { get; set; }
            public ManualResetEvent ManualResetEvent { get; set; }
            public Task TaskRunner { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
        }
    }
}
