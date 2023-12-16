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

namespace southafricantaxtool.DAL.Stores;

public class MdbImportantDateStore : IStore<ImportantDate>
{
    private readonly IMongoCollection<MdbImportantDates> _importantDatesCollection;
    private readonly ILogger<MdbImportantDateStore> _logger;

    private readonly IStore<ImportantDate> _redisCache;
    

    public MdbImportantDateStore(
        IOptions<MongoDbConfiguration> sarsDatabaseSettings, ILogger<MdbImportantDateStore> logger, IDistributedCache cache, [FromKeyedServices("redis")] IStore<ImportantDate> redisCache)
    {
        _logger = logger;
        _redisCache = redisCache;
        
        var mongoClient = new MongoClient(
            sarsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            sarsDatabaseSettings.Value.DatabaseName);

        _importantDatesCollection = mongoDatabase.GetCollection<MdbImportantDates>(
            MongoDbConsts.ImportDatesCollectionName);
    }

    public async Task<List<ImportantDate>?> GetAsync(Func<ImportantDate, bool>? filter = null)
    {
        var cachedData = await _redisCache.GetAsync(filter);

        if (cachedData != null) return cachedData;
        
        var doc = await _importantDatesCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            await _redisCache.SetAsync(doc.ImportantDates);
            
            if (filter == null) return doc.ImportantDates;
            
            var dates = doc.ImportantDates.Where(filter).ToList();
            return dates;

        }
        
        _logger.LogError("No important dates found in MongoDB. The Worker services might not be running");
        throw new InvalidOperationException("No important dates found");


    }

    public async Task SetAsync(List<ImportantDate> importantDates)
    {
        var doc = await _importantDatesCollection.Find(_ => true).FirstOrDefaultAsync();
        
        if (doc == null)
        {
            doc = new MdbImportantDates
            {
                ImportantDates = importantDates
            };
            await _importantDatesCollection.InsertOneAsync(doc);
            await _redisCache.SetAsync(importantDates);
            return;
        }

        doc.ImportantDates = importantDates;

        await _redisCache.SetAsync(doc.ImportantDates);

        await _importantDatesCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }

}