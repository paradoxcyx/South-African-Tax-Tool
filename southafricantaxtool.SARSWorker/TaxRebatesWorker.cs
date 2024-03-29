﻿using southafricantaxtool.Interface;
using southafricantaxtool.Interface.Models;
using southafricantaxtool.SARSScraper;

namespace southafricantaxtool.SARSWorker;

public class TaxRebatesWorker(ILogger<TaxRebatesWorker> logger, IStore<TaxRebate> mdbTaxRebateService, TaxRebateScraper taxRebateScraper) : BackgroundService
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