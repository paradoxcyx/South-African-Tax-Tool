using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.Interface.Services;
using southafricantaxtool.Interface.Models;
using Newtonsoft.Json;

namespace southafricantaxtool.DAL.Stores;

public class MdbTaxRebateStore : Store, IStore<TaxRebate>
{
    private readonly IMongoCollection<MdbTaxRebates> _rebatesCollection;
    private readonly ILogger<MdbTaxRebateStore> _logger;
    private readonly IDistributedCache _cache;
    
    protected override string RedisKey => "tax-rebates";
    
    public MdbTaxRebateStore(
        IOptions<MongoDbConfiguration> sarsDatabaseSettings, ILogger<MdbTaxRebateStore> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
        var mongoClient = new MongoClient(
            sarsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            sarsDatabaseSettings.Value.DatabaseName);

        _rebatesCollection = mongoDatabase.GetCollection<MdbTaxRebates>(
            MongoDbConsts.TaxRebatesCollectionName);
    }

    public async Task<List<TaxRebate>> GetAsync(Func<TaxRebate, bool>? filter = null)
    {
        var cachedData = await _cache.GetAsync(RedisKey);

        if (cachedData != null)
        {
            var json = Encoding.UTF8.GetString(cachedData);
            var taxRebates = JsonConvert.DeserializeObject<List<TaxRebate>>(json);

            if (taxRebates != null)
            {
                return taxRebates;
            }
        }
        
        var doc = await _rebatesCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            await UpdateRedisCache(doc.TaxRebates);
            return doc.TaxRebates;
        }
        
        _logger.LogError("No tax rebates found in MongoDB. The Worker services might not be running");
        throw new InvalidOperationException("No tax rebates found");
    }
    
    public async Task SetAsync(List<TaxRebate> taxRebates)
    {
        var doc = await _rebatesCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc == null)
        {
            doc = new MdbTaxRebates
            {
                TaxRebates = taxRebates
            };
            await _rebatesCollection.InsertOneAsync(doc);
            await UpdateRedisCache(taxRebates);
            return;
        }

        doc.TaxRebates = taxRebates;

        await UpdateRedisCache(taxRebates);
        
        await _rebatesCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }
    
    private async Task UpdateRedisCache(List<TaxRebate> taxRebates)
    {
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
        };

        await _cache.SetAsync(RedisKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taxRebates)), cacheOptions);
    }
}