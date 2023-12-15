using Microsoft.Extensions.DependencyInjection;
using southafricantaxtool.DAL.Stores;
using southafricantaxtool.Interface.Services;

namespace southafricantaxtool.DAL;

public static class DependencyInjection
{
    public static void AddDataAccessLayer(this IServiceCollection services)
    {
        services.AddScoped<ITaxBracketStore, MdbTaxBracketStore>();
        services.AddScoped<ITaxRebateStore, MdbTaxRebateStore>();
        services.AddScoped<IImportantDateStore, MdbImportantDateStore>();
    }
}