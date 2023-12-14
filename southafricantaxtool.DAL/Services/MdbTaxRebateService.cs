using Microsoft.Extensions.Options;
using MongoDB.Driver;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.DAL.Services;

public class MdbTaxRebateService
{
    private readonly IMongoCollection<MdbTaxRebates> _rebatesCollection;

    public MdbTaxRebateService(
        IOptions<MongoDbConfiguration> bookStoreDatabaseSettings)
    {
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

        if (doc != null)
        {
            return doc.TaxRebates;
        }

        var taxRebates = await Scraper.Scraper.RetrieveTaxRebates();

        var insertDoc = new MdbTaxRebates
        {
            TaxRebates = taxRebates
        };

        await _rebatesCollection.InsertOneAsync(insertDoc);
        return taxRebates;
    }
}