using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using southafricantaxtool.Interface.Models;
using southafricantaxtool.Interface;

namespace southafricantaxtool.Caching.Stores;

public class RedisTaxBracketStore(IDistributedCache cache, ILogger<RedisTaxBracketStore> logger)
    : IStore<TaxBracket>
{
    private const string RedisKey = "tax-brackets";

    public async Task<List<TaxBracket>?> GetAsync(Func<TaxBracket, bool>? filter = null)
    {
        var cachedData = await cache.GetAsync(RedisKey);

        if (cachedData == null) return null;
        
        var json = Encoding.UTF8.GetString(cachedData);
        var dates = JsonConvert.DeserializeObject<List<TaxBracket>>(json);

        if (dates == null)
        {
            logger.LogError("Unable to deserialize data from redis cache");
            return null;
        }
        if (filter == null) return dates;

        dates = dates.Where(filter).ToList();
            
        return dates;

    }

    public Task SetAsync(List<TaxBracket> data)
    {
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
        };

        return cache.SetAsync(RedisKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)), cacheOptions);
    }
}