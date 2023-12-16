using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.Interface;
using southafricantaxtool.Interface.Models;
using Newtonsoft.Json;

namespace southafricantaxtool.DAL.Stores;

public class MdbTaxRebateStore : IStore<TaxRebate>
{
    private readonly IMongoCollection<MdbTaxRebates> _rebatesCollection;
    private readonly ILogger<MdbTaxRebateStore> _logger;
    private readonly IStore<TaxRebate> _redisCache;
    
    public MdbTaxRebateStore(
        IOptions<MongoDbConfiguration> sarsDatabaseSettings, ILogger<MdbTaxRebateStore> logger, [FromKeyedServices("redis")] IStore<TaxRebate> redisCache)
    {
        _logger = logger;
        _redisCache = redisCache;
        var mongoClient = new MongoClient(
            sarsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            sarsDatabaseSettings.Value.DatabaseName);

        _rebatesCollection = mongoDatabase.GetCollection<MdbTaxRebates>(
            MongoDbConsts.TaxRebatesCollectionName);
    }

    public async Task<List<TaxRebate>?> GetAsync(Func<TaxRebate, bool>? filter = null)
    {
        var cachedData = await _redisCache.GetAsync(filter);

        if (cachedData != null) return cachedData;
        
        var doc = await _rebatesCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            await _redisCache.SetAsync(doc.TaxRebates);
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
            await _redisCache.SetAsync(taxRebates);
            return;
        }

        doc.TaxRebates = taxRebates;

        await _redisCache.SetAsync(taxRebates);
        
        await _rebatesCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }
    
}