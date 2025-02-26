using DomainBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Infrastructure.Object
{
    public static class JobApiProcessManager<T>
    {
        static Channel<T> channel = Channel.CreateUnbounded<T>();
        static Func<T, Task> ProcessFunc;
        static HashSet<T> _publishedIds;
        static Lock _lock = new Lock();
        static SemaphoreSlim sem = new SemaphoreSlim(1);
        public static void RegisterProcessFunc(Func<T, Task> func, IEqualityComparer<T> comparer = null)
        {
            if (ProcessFunc == null)
            {
                lock (_lock)
                {
                    if (ProcessFunc == null)
                    {
                        ProcessFunc = func;
                        _publishedIds = new HashSet<T>(comparer ?? EqualityComparer<T>.Default);
                        _ = ReadChannel();//自动启动订阅器
                    }
                }
            }
        }
        public static async Task SendEvent(T item)
        {
            await sem.WaitAsync();
            try
            {
                if (!_publishedIds.Contains(item))
                {
                    await channel.Writer.WriteAsync(item);
                    _publishedIds.Add(item);
                }
            }
            finally
            {
                sem.Release();
            }
        }
        private static async Task ReadChannel()
        {
            while(await channel.Reader.WaitToReadAsync())
            {
                if (channel.Reader.TryRead(out var item))
                {
                    if (ProcessFunc != null)
                    {
                        //需要开发者自行在ProcessFunc内部处理异常，不要抛出
                        await ProcessFunc(item);
                        _publishedIds.Remove(item);
                    }
                }
            }
        }
    }
}
