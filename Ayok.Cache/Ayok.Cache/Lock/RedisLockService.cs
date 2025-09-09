using System.Diagnostics;
using Ayok.Cache.Caches;
using Ayok.Cache.Enums;

namespace Ayok.Cache.Lock
{
    public class RedisLockService : ILockService
    {
        private readonly ICacheService cacheService;

        public RedisLockService(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        public T ActionLock<T>(
            string lockKey,
            Func<T> action,
            LockTypeEnum lockTypeEnum = LockTypeEnum.Wait,
            int secondsTimeout = 180
        )
        {
            string key = "lock:" + lockKey;
            string value = Guid.NewGuid().ToString();
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            int num = secondsTimeout * 1000;
            try
            {
                bool flag;
                do
                {
                    flag = cacheService.AcquireLock(
                        key,
                        value,
                        TimeSpan.FromSeconds(secondsTimeout)
                    );
                    if (!flag)
                    {
                        if (lockTypeEnum == LockTypeEnum.Error)
                        {
                            throw new Exception("获取锁失败！");
                        }
                        if (stopwatch.ElapsedMilliseconds > num)
                        {
                            throw new Exception("获取锁超时！");
                        }
                        Thread.Sleep(100);
                    }
                } while (!flag);
                return action();
            }
            finally
            {
                cacheService.ReleaseLock(key, value);
            }
        }

        public async Task<T> ActionLockAsync<T>(
            string lockKey,
            Func<Task<T>> action,
            LockTypeEnum lockTypeEnum = LockTypeEnum.Wait,
            int secondsTimeout = 180
        )
        {
            string lockName = "lock:" + lockKey;
            string token = Guid.NewGuid().ToString();
            Stopwatch stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            int millisecondsTimeout = secondsTimeout * 1000;
            T result;
            try
            {
                bool isAcquireLock;
                do
                {
                    isAcquireLock = await cacheService.AcquireLockAsync(
                        lockName,
                        token,
                        TimeSpan.FromSeconds(secondsTimeout)
                    );
                    if (!isAcquireLock)
                    {
                        if (lockTypeEnum == LockTypeEnum.Error)
                        {
                            throw new Exception("获取锁失败！");
                        }
                        if (stopwatch.ElapsedMilliseconds > millisecondsTimeout)
                        {
                            throw new Exception("获取锁超时！");
                        }
                        await Task.Delay(100);
                    }
                } while (!isAcquireLock);
                result = await action();
            }
            finally
            {
                await cacheService.ReleaseLockAsync(lockName, token);
            }
            return result;
        }
    }
}
