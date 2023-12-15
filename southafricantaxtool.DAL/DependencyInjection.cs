using Microsoft.Extensions.DependencyInjection;
using southafricantaxtool.DAL.Services;

namespace southafricantaxtool.DAL;

public static class DependencyInjection
{
    public static void AddDataAccessLayer(this IServiceCollection services)
    {
        services.AddSingleton<MdbTaxBracketStore>();
        services.AddSingleton<MdbTaxRebateStore>();
        services.AddSingleton<MdbImportantDateService>();
    }
}