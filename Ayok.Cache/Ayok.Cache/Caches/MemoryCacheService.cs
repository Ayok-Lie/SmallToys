using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace Ayok.Cache.Caches
{
    public class MemoryCacheService : ICacheService, IDisposable
    {
        private IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        private List<string> GetCacheKeys()
        {
            cache.GetType();
            object? value = cache
                .GetType()
                .GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(cache);
            List<string> list = new List<string>();
            if (!(value is IDictionary dictionary))
            {
                return list;
            }
            foreach (DictionaryEntry item in dictionary)
            {
                list.Add(item.Key.ToString());
            }
            return list;
        }

        public bool Add<T>(string key, T value, TimeSpan? expiry = null, int? dbIndex = null)
        {
            cache.Set(key, value, new MemoryCacheEntryOptions { SlidingExpiration = expiry });
            return true;
        }

        public bool AddString(
            string key,
            string value,
            TimeSpan? expiry = null,
            int? dbIndex = null
        )
        {
            cache.Set(key, value, new MemoryCacheEntryOptions { SlidingExpiration = expiry });
            return true;
        }

        public bool Exists(string key, int? dbIndex = null)
        {
            object value;
            return cache.TryGetValue(key, out value);
        }

        public T Get<T>(string key, int? dbIndex = null)
        {
            return cache.Get<T>(key);
        }

        public IEnumerable<string> GetAllKey(string pattern, int? dbIndex = null)
        {
            return (from k in GetCacheKeys() where Regex.IsMatch(k, pattern) select k)
                .ToList()
                .AsReadOnly();
        }

        public string GetString(string key, int? dbIndex = null)
        {
            return cache.Get(key)?.ToString();
        }

        public bool Remove(string key, int? dbIndex = null)
        {
            cache.Remove(key);
            return true;
        }

        public bool KeyExpire(string key, TimeSpan? expiry, int? dbIndex = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetStringAsync(string key, int? dbIndex = null)
        {
            throw new NotImplementedException();
        }

        Task<long> ICacheService.GetIncrementAsync(string key, int? dbIndex)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string key, int? dbIndex = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddAsync<T>(
            string key,
            T value,
            TimeSpan? expiry = null,
            int? dbIndex = null
        )
        {
            throw new NotImplementedException();
        }

        public Task<bool> AddStringAsync(
            string key,
            string value,
            TimeSpan? expiry = null,
            int? dbIndex = null
        )
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAsync<T>(string key, int? dbIndex = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> AcquireLockAsync(
            string key,
            string value,
            TimeSpan lockExpiry,
            int? dbIndex = null
        )
        {
            return Task.FromResult(Monitor.TryEnter(key, lockExpiry));
        }

        public Task<bool> ReleaseLockAsync(string key, string value, int? dbIndex = null)
        {
            Monitor.Exit(key);
            return Task.FromResult(result: true);
        }

        public bool AcquireLock(string key, string value, TimeSpan lockExpiry, int? dbIndex = null)
        {
            return Monitor.TryEnter(key, lockExpiry);
        }

        public bool ReleaseLock(string key, string value, int? dbIndex = null)
        {
            Monitor.Exit(key);
            return true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
