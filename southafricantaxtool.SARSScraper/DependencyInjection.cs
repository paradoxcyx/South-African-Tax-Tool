using Microsoft.Extensions.DependencyInjection;

namespace southafricantaxtool.SARSScraper;

public static class DependencyInjection
{
    public static void AddScrapers(this IServiceCollection services)
    {
        services.AddSingleton<TaxBracketScraper>();
        services.AddSingleton<TaxRebateScraper>();
        services.AddSingleton<ImportantDateScraper>();
    }
}