using System.Text;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.Interface;
using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.DAL.Stores;

public class MdbTaxBracketStore : ITaxBracketStore
{
    private readonly IMongoCollection<MdbTaxBrackets> _bracketsCollection;
    private readonly ILogger<MdbTaxBracketStore> _logger;
    private readonly IDistributedCache _cache;
    private const string RedisKey = "tax-brackets";
    
    public MdbTaxBracketStore(
        IOptions<MongoDbConfiguration> sarsDatabaseSettings, ILogger<MdbTaxBracketStore> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
        var mongoClient = new MongoClient(
            sarsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            sarsDatabaseSettings.Value.DatabaseName);

        _bracketsCollection = mongoDatabase.GetCollection<MdbTaxBrackets>(
            MongoDbConsts.TaxBracketsCollectionName);
    }

    public async Task<List<TaxBracket>> GetAsync()
    {
        var cachedData = await _cache.GetAsync(RedisKey);

        if (cachedData != null)
        {
            var json = Encoding.UTF8.GetString(cachedData);
            var taxBrackets = JsonConvert.DeserializeObject<List<TaxBracket>>(json);

            if (taxBrackets != null)
            {
                return taxBrackets;
            }
        }
        
        var doc = await _bracketsCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            await UpdateRedisCache(doc.TaxBrackets);
            return doc.TaxBrackets;
        }
        
        _logger.LogError("No tax brackets found in MongoDB. The Worker services might not be running");
        throw new InvalidOperationException("No tax brackets found");


    }

    public async Task SetAsync(List<TaxBracket> taxBrackets)
    {
        var doc = await _bracketsCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc == null)
        {
            doc = new MdbTaxBrackets
            {
                TaxBrackets = taxBrackets
            };
            await _bracketsCollection.InsertOneAsync(doc);
            await UpdateRedisCache(taxBrackets);
            return;
        }

        doc.TaxBrackets = taxBrackets;

        await UpdateRedisCache(doc.TaxBrackets);
        
        await _bracketsCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }
    
    private async Task UpdateRedisCache(List<TaxBracket> taxBrackets)
    {
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
        };

        await _cache.SetAsync(RedisKey, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taxBrackets)), cacheOptions);
    }

}
