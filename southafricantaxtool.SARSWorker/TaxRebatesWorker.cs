using southafricantaxtool.DAL.Services;
using southafricantaxtool.SARSScraper;

namespace southafricantaxtool.SARSWorker;

public class TaxRebatesWorker(ILogger<TaxRebatesWorker> logger, MdbTaxRebateService mdbTaxRebateService, TaxRebateScraper taxRebateScraper) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            
            var taxRebates = await taxRebateScraper.RetrieveTaxRebates();

            await mdbTaxRebateService.SetAsync(taxRebates);
            logger.LogInformation("Updated {count} tax rebates to MongoDB", taxRebates.Count);

            await Task.Delay(int.MaxValue, stoppingToken);
        }
    }
}