using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.DAL.Services;

public class MdbTaxBracketService
{
    private readonly IMongoCollection<MdbTaxBrackets> _bracketsCollection;
    private readonly ILogger<MdbTaxBracketService> _logger;
    
    public MdbTaxBracketService(
        IOptions<MongoDbConfiguration> bookStoreDatabaseSettings, ILogger<MdbTaxBracketService> logger)
    {
        _logger = logger;
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _bracketsCollection = mongoDatabase.GetCollection<MdbTaxBrackets>(
            MongoDbConsts.TaxBracketsCollectionName);
    }

    public async Task<List<TaxBracket>> GetAsync()
    {
        var doc = await _bracketsCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null) return doc.TaxBrackets;
        
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
            return;
        }

        doc.TaxBrackets = taxBrackets;

        await _bracketsCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }
}
