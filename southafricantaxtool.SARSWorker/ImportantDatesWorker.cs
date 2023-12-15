using southafricantaxtool.DAL.Stores;
using southafricantaxtool.Interface.Services;
using southafricantaxtool.SARSScraper;

namespace southafricantaxtool.SARSWorker;

public class ImportantDatesWorker(ILogger<ImportantDatesWorker> logger, IImportantDateStore mdbImportantDateService, ImportantDateScraper importantDateScraper) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            
            var dates = await importantDateScraper.RetrieveImportantDates();

            await mdbImportantDateService.SetAsync(dates);
            logger.LogInformation("Updated {count} important dates to MongoDB", dates.Count);

            await Task.Delay(int.MaxValue, stoppingToken);
        }
    }
}