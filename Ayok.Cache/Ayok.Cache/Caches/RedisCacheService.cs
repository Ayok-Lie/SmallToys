using Ayok.Cache.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Ayok.Cache.Caches
{
    public class RedisCacheService : ICacheService
    {
        private readonly ConnectionMultiplexer redis;

        private readonly CacheOptions options;

        public RedisCacheService(IOptions<CacheOptions> options)
        {
            redis = ConnectionMultiplexer.Connect(options.Value.RedisConnectionString);
            this.options = options.Value;
        }

        private IDatabase GetDatabase(int? dbIndex)
        {
            if (!dbIndex.HasValue)
            {
                return redis.GetDatabase(options.RedisDefaultDatabase);
            }
            return redis.GetDatabase(dbIndex.Value);
        }

        public bool Add<T>(string key, T value, TimeSpan? expiry = null, int? dbIndex = null)
        {
            string value2 = JsonConvert.SerializeObject(value);
            return AddString(key, value2, expiry, dbIndex);
        }

        public async Task<bool> AddAsync<T>(
            string key,
            T value,
            TimeSpan? expiry = null,
            int? dbIndex = null
        )
        {
            string value2 = JsonConvert.SerializeObject(value);
            return await AddStringAsync(key, value2, expiry, dbIndex);
        }

        public async Task<bool> AddStringAsync(
            string key,
            string value,
            TimeSpan? expiry = null,
            int? dbIndex = null
        )
        {
            return await GetDatabase(dbIndex).StringSetAsync(key, value, expiry);
        }

        public bool AddString(
            string key,
            string value,
            TimeSpan? expiry = null,
            int? dbIndex = null
        )
        {
            return GetDatabase(dbIndex).StringSet(key, value, expiry);
        }

        public T Get<T>(string key, int? dbIndex = null)
        {
            string text = GetString(key, dbIndex);
            if (text != null)
            {
                return JsonConvert.DeserializeObject<T>(text);
            }
            return default(T);
        }

        public async Task<T> GetAsync<T>(string key, int? dbIndex = null)
        {
            string text = await GetDatabase(dbIndex).StringGetAsync(key);
            if (text != null)
            {
                return JsonConvert.DeserializeObject<T>(text);
            }
            return default(T);
        }

        public string GetString(string key, int? dbIndex = null)
        {
            return GetDatabase(dbIndex).StringGet(key);
        }

        public async Task<string> GetStringAsync(string key, int? dbIndex = null)
        {
            return await GetDatabase(dbIndex).StringGetAsync(key);
        }

        public bool Remove(string key, int? dbIndex = null)
        {
            return GetDatabase(dbIndex).KeyDelete(key);
        }

        public IEnumerable<string> GetAllKey(string pattern, int? dbIndex = null)
        {
            List<string> list = new List<string>();
            LuaScript script = LuaScript.Prepare("return redis.call('keys',@pattern)");
            RedisResult redisResult = GetDatabase(dbIndex).ScriptEvaluate(script, new { pattern });
            if (!redisResult.IsNull)
            {
                RedisKey[] array = (RedisKey[]?)redisResult;
                foreach (RedisKey redisKey in array)
                {
                    list.Add(redisKey);
                }
            }
            return list;
        }

        public bool Exists(string key, int? dbIndex = null)
        {
            return GetDatabase(dbIndex).KeyExists(key);
        }

        public async Task<bool> ExistsAsync(string key, int? dbIndex = null)
        {
            return await GetDatabase(dbIndex).KeyExistsAsync(key);
        }

        public bool KeyExpire(string key, TimeSpan? expiry, int? dbIndex = null)
        {
            return GetDatabase(dbIndex).KeyExpire(key, expiry);
        }

        public async Task<long> GetIncrementAsync(string key, int? dbIndex = null)
        {
            return await GetDatabase(dbIndex).StringIncrementAsync(key, 1L);
        }

        public async Task<bool> AcquireLockAsync(
            string key,
            string value,
            TimeSpan lockExpiry,
            int? dbIndex = null
        )
        {
            return await GetDatabase(dbIndex).LockTakeAsync(key, value, lockExpiry);
        }

        public bool AcquireLock(string key, string value, TimeSpan lockExpiry, int? dbIndex = null)
        {
            return GetDatabase(dbIndex).LockTake(key, value, lockExpiry);
        }

        public async Task<bool> ReleaseLockAsync(string key, string value, int? dbIndex = null)
        {
            return await GetDatabase(dbIndex).LockReleaseAsync(key, value);
        }

        public bool ReleaseLock(string key, string value, int? dbIndex = null)
        {
            return GetDatabase(dbIndex).LockRelease(key, value);
        }
    }
}
