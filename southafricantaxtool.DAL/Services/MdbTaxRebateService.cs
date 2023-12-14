using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.DAL.Services;

public class MdbTaxRebateService
{
    private readonly IMongoCollection<MdbTaxRebates> _rebatesCollection;
    private readonly ILogger<MdbTaxRebateService> _logger;
    
    public MdbTaxRebateService(
        IOptions<MongoDbConfiguration> bookStoreDatabaseSettings, ILogger<MdbTaxRebateService> logger)
    {
        _logger = logger;
        var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            bookStoreDatabaseSettings.Value.DatabaseName);

        _rebatesCollection = mongoDatabase.GetCollection<MdbTaxRebates>(
            MongoDbConsts.TaxRebatesCollectionName);
    }

    public async Task<List<TaxRebate>> GetAsync()
    {
        var doc = await _rebatesCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null) return doc.TaxRebates;
        
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
            return;
        }

        doc.TaxRebates = taxRebates;

        await _rebatesCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }
}