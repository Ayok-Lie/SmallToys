using Ayok.Cache.Enums;

namespace Ayok.Cache.Lock
{
    public interface ILockService
    {
        T ActionLock<T>(
            string lockKey,
            Func<T> action,
            LockTypeEnum lockTypeEnum = LockTypeEnum.Wait,
            int secondsTimeout = 180
        );

        Task<T> ActionLockAsync<T>(
            string lockKey,
            Func<Task<T>> action,
            LockTypeEnum lockTypeEnum = LockTypeEnum.Wait,
            int secondsTimeout = 180
        );
    }
}
