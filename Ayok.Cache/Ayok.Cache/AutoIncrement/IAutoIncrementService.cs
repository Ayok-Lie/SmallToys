namespace Ayok.Cache.AutoIncrement
{
    public interface IAutoIncrementService
    {
        Task<long> GenerateIdAsync(string key, int? dbIndex = null, int secondsTimeout = 3);
    }
}
