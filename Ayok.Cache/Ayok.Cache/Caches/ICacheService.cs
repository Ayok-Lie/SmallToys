namespace Ayok.Cache.Caches
{
    public interface ICacheService
    {
        bool Add<T>(string key, T value, TimeSpan? expiry = null, int? dbIndex = null);

        Task<bool> AddAsync<T>(string key, T value, TimeSpan? expiry = null, int? dbIndex = null);

        bool AddString(string key, string value, TimeSpan? expiry = null, int? dbIndex = null);

        Task<bool> AddStringAsync(
            string key,
            string value,
            TimeSpan? expiry = null,
            int? dbIndex = null
        );

        T Get<T>(string key, int? dbIndex = null);

        Task<T> GetAsync<T>(string key, int? dbIndex = null);

        string GetString(string key, int? dbIndex = null);

        Task<string> GetStringAsync(string key, int? dbIndex = null);

        Task<long> GetIncrementAsync(string key, int? dbIndex = null);

        bool AcquireLock(string key, string value, TimeSpan lockExpiry, int? dbIndex = null);

        Task<bool> AcquireLockAsync(
            string key,
            string value,
            TimeSpan lockExpiry,
            int? dbIndex = null
        );

        bool ReleaseLock(string key, string value, int? dbIndex = null);

        Task<bool> ReleaseLockAsync(string key, string value, int? dbIndex = null);

        bool Remove(string key, int? dbIndex = null);

        IEnumerable<string> GetAllKey(string pattern, int? dbIndex = null);

        bool Exists(string key, int? dbIndex = null);

        Task<bool> ExistsAsync(string key, int? dbIndex = null);

        bool KeyExpire(string key, TimeSpan? expiry, int? dbIndex = null);
    }
}
