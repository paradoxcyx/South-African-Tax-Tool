using southafricantaxtool.DAL.Stores;
using southafricantaxtool.Interface;
using southafricantaxtool.Interface.Models;
using southafricantaxtool.SARSScraper;

namespace southafricantaxtool.SARSWorker;

public class TaxBracketsWorker(ILogger<TaxBracketsWorker> logger, IStore<TaxBracket> mdbTaxBracketService, TaxBracketScraper taxBracketScraper) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            
            var taxBrackets = await taxBracketScraper.RetrieveTaxBrackets();

            await mdbTaxBracketService.SetAsync(taxBrackets);
            logger.LogInformation("Updated {count} tax brackets to MongoDB", taxBrackets.Count);

            await Task.Delay(int.MaxValue, stoppingToken);
        }
    }
}