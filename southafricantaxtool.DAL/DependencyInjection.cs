using Microsoft.Extensions.DependencyInjection;
using southafricantaxtool.DAL.Stores;
using southafricantaxtool.Interface.Models;
using southafricantaxtool.Interface.Services;

namespace southafricantaxtool.DAL;

public static class DependencyInjection
{
    public static void AddDataAccessLayer(this IServiceCollection services)
    {
        services.AddScoped<IStore<TaxBracket>, MdbTaxBracketStore>();
        services.AddScoped<IStore<TaxRebate>, MdbTaxRebateStore>();
        services.AddScoped<IStore<ImportantDate>, MdbImportantDateStore>();
    }
}