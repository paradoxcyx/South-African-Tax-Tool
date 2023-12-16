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
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace southafricantaxtool.DAL.Stores;

public class MdbTaxBracketStore : IStore<TaxBracket>
{
    private readonly IMongoCollection<MdbTaxBrackets> _bracketsCollection;
    private readonly ILogger<MdbTaxBracketStore> _logger;
    private readonly IStore<TaxBracket> _redisCache;
    
    
    public MdbTaxBracketStore(
        IOptions<MongoDbConfiguration> sarsDatabaseSettings, ILogger<MdbTaxBracketStore> logger, [FromKeyedServices("redis")] IStore<TaxBracket> redisCache)
    {
        _logger = logger;
        _redisCache = redisCache;
        var mongoClient = new MongoClient(
            sarsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            sarsDatabaseSettings.Value.DatabaseName);

        _bracketsCollection = mongoDatabase.GetCollection<MdbTaxBrackets>(
            MongoDbConsts.TaxBracketsCollectionName);
    }

    public async Task<List<TaxBracket>?> GetAsync(Func<TaxBracket, bool>? filter = null)
    {
        var cachedData = await _redisCache.GetAsync(filter);

        if (cachedData != null) return cachedData;
        
        var doc = await _bracketsCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
            await _redisCache.SetAsync(doc.TaxBrackets);
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
            await _redisCache.SetAsync(taxBrackets);
            return;
        }

        doc.TaxBrackets = taxBrackets;

        await _redisCache.SetAsync(doc.TaxBrackets);
        
        await _bracketsCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }
    

}
