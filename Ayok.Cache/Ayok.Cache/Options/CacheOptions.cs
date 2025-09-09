namespace Ayok.Cache.Options
{
    public class CacheOptions
    {
        public short CacheType { get; set; } = 1;

        public string RedisConnectionString { get; set; }

        public int RedisDefaultDatabase { get; set; }

        public short LockType { get; set; } = 1;
    }
}
