namespace Ayok.Cache.AutoIncrement
{
    public class RedisIncrementService : IAutoIncrementService
    {
        private readonly ICacheService cacheService;

        private readonly ILockService lockService;

        private readonly CacheOptions options;

        public RedisIncrementService(
            ICacheService cacheService,
            ILockService lockService,
            IOptions<CacheOptions> options
        )
        {
            this.cacheService = cacheService;
            this.lockService = lockService;
            this.options = options.Value;
        }

        public async Task<long> GenerateIdAsync(
            string key,
            int? dbIndex = null,
            int secondsTimeout = 3
        )
        {
            return await lockService.ActionLockAsync(
                key,
                async () => await cacheService.GetIncrementAsync(key, dbIndex),
                LockTypeEnum.Wait,
                secondsTimeout
            );
        }
    }
}
