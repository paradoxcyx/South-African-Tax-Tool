using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using southafricantaxtool.DAL.Configuration;
using southafricantaxtool.DAL.Models;
using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.DAL.Services;

public class MdbImportantDateService
{
    private readonly IMongoCollection<MdbImportantDates> _importantDatesCollection;
    private readonly ILogger<MdbImportantDateService> _logger;
    
    public MdbImportantDateService(
        IOptions<MongoDbConfiguration> sarsDatabaseSettings, ILogger<MdbImportantDateService> logger)
    {
        _logger = logger;
        var mongoClient = new MongoClient(
            sarsDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            sarsDatabaseSettings.Value.DatabaseName);

        _importantDatesCollection = mongoDatabase.GetCollection<MdbImportantDates>(
            MongoDbConsts.ImportDatesCollectionName);
    }

    public async Task<List<ImportantDate>> GetAsync(Func<ImportantDate, bool>? filter = null)
    {
        var doc = await _importantDatesCollection.Find(_ => true).FirstOrDefaultAsync();

        if (doc != null)
        {
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
            return;
        }

        doc.ImportantDates = importantDates;

        await _importantDatesCollection.FindOneAndReplaceAsync(d => d.Id == doc.Id, doc);
    }
}