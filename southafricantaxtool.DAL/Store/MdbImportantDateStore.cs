using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.Interface.Services;
using southafricantaxtool.Interface.Models;
using System.Text.Json;

namespace southafricantaxtool.DAL.Stores;

public class MdbImportantDateStore : IImportantDateStore
{
    private readonly IMongoCollection<MdbImportantDates> _importantDatesCollection;
    private readonly ILogger<MdbImportantDateStore> _logger;
    private readonly IDistributedCache _cache;
    private const string RedisKey = "important-dates";
    
    public MdbImportantDateStore(
        IOptions<MongoDbConfiguration> sarsDatabaseSettings, ILogger<MdbImportantDateStore> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
        var mongoClient = new MongoClient(
            sarsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            sarsDatabaseSettings.Value.DatabaseName);

        _importantDatesCollection = mongoDatabase.GetCollection<MdbImportantDates>(
            MongoDbConsts.ImportDatesCollectionName);
    }

    public async Task<List<ImportantDate>> GetAsync(Func<ImportantDate, bool>? filter = null)
    {
        var cachedData = await _cache.GetAsync(RedisKey);

        if (cachedData != null)
        {
            var json = Encoding.UTF8.GetString(cachedData);
            var dates = JsonSerializer.Deserialize<List<ImportantDate>>(json);

            if (dates != null)
            {
                if (filter == null) return dates;

                dates = dates.Where(filter).ToList();
            
                return dates;
            }
        }
        
        
        var doc = await _importantDatesCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            await UpdateRedisCache(doc.ImportantDates);
            
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
            await UpdateRedisCache(importantDates);
            return;
        }

        doc.ImportantDates = importantDates;

        await UpdateRedisCache(doc.ImportantDates);

        await _importantDatesCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }

    private async Task UpdateRedisCache(List<ImportantDate> importantDates)
    {
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
        };

        await _cache.SetAsync(RedisKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(importantDates)), cacheOptions);
    }
}