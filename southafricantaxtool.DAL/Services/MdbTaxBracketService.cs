using Microsoft.Extensions.Options;
using MongoDB.Driver;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.DAL.Services;

public class MdbTaxBracketService
{
    private readonly IMongoCollection<MdbTaxBrackets> _bracketsCollection;

    public MdbTaxBracketService(
        IOptions<MongoDbConfiguration> bookStoreDatabaseSettings)
    {
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

        if (doc != null)
        {
            return doc.TaxBrackets;
        }

        var taxBrackets = await Scraper.Scraper.RetrieveTaxBrackets();

        var insertDoc = new MdbTaxBrackets
        {
            TaxBrackets = taxBrackets
        };

        await _bracketsCollection.InsertOneAsync(insertDoc);
        return taxBrackets;
    }
}
