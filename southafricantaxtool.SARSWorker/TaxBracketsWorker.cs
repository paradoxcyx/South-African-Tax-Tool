namespace southafricantaxtool.SARSWorker;

public class TaxBracketsWorker(ILogger<TaxBracketsWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var taxBrackets = await Scraper.Scraper.RetrieveTaxData()
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

            logger.LogInformation("logging for: {duration}", int.MaxValue);
            await Task.Delay(int.MaxValue, stoppingToken);
        }
    }
}