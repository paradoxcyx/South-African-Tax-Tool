using Microsoft.Extensions.DependencyInjection;
using southafricantaxtool.DAL.Services;

namespace southafricantaxtool.DAL;

public static class DependencyInjection
{
    public static void AddDAL(this IServiceCollection services)
    {
        services.AddSingleton<MdbTaxBracketService>();
        services.AddSingleton<MdbTaxRebateService>();
        services.AddSingleton<MdbImportantDateService>();
    }
}