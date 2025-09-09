using System.Collections.Concurrent;
using Ayok.Cache.Enums;

namespace Ayok.Cache.Lock
{
    public class ThreadLockService : ILockService
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreDict =
            new ConcurrentDictionary<string, SemaphoreSlim>();

        public T ActionLock<T>(
            string lockKey,
            Func<T> action,
            LockTypeEnum lockTypeEnum = LockTypeEnum.Wait,
            int secondsTimeout = 180
        )
        {
            SemaphoreSlim orAdd = _semaphoreDict.GetOrAdd(
                lockKey,
                (string _) => new SemaphoreSlim(1, 1)
            );
            bool flag = false;
            try
            {
                if (lockTypeEnum == LockTypeEnum.Error)
                {
                    flag = orAdd.Wait(0);
                    if (!flag)
                    {
                        throw new Exception("获取锁失败！");
                    }
                }
                else
                {
                    flag = orAdd.Wait(TimeSpan.FromSeconds(secondsTimeout));
                    if (!flag)
                    {
                        throw new Exception("获取锁超时！");
                    }
                }
                return action();
            }
            finally
            {
                if (flag)
                {
                    orAdd.Release();
                }
            }
        }

        public async Task<T> ActionLockAsync<T>(
            string lockKey,
            Func<Task<T>> action,
            LockTypeEnum lockTypeEnum = LockTypeEnum.Wait,
            int secondsTimeout = 180
        )
        {
            SemaphoreSlim semaphore = _semaphoreDict.GetOrAdd(
                lockKey,
                (string _) => new SemaphoreSlim(1, 1)
            );
            bool isAcquireLock = false;
            try
            {
                if (lockTypeEnum == LockTypeEnum.Error)
                {
                    isAcquireLock = await semaphore
                        .WaitAsync(0)
                        .ConfigureAwait(continueOnCapturedContext: false);
                    if (!isAcquireLock)
                    {
                        throw new Exception("获取锁失败！");
                    }
                }
                else
                {
                    isAcquireLock = await semaphore
                        .WaitAsync(TimeSpan.FromSeconds(secondsTimeout))
                        .ConfigureAwait(continueOnCapturedContext: false);
                    if (!isAcquireLock)
                    {
                        throw new Exception("获取锁超时！");
                    }
                }
                return await action().ConfigureAwait(continueOnCapturedContext: false);
            }
            finally
            {
                if (isAcquireLock)
                {
                    semaphore.Release();
                }
            }
        }
    }
}
