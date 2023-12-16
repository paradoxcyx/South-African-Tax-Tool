using Microsoft.Extensions.DependencyInjection;
using southafricantaxtool.Caching.Stores;
using southafricantaxtool.Interface.Models;
using southafricantaxtool.Interface;

namespace southafricantaxtool.Caching;

public static class DependencyInjection
{
    public static void AddCaching(this IServiceCollection services)
    {
        services.AddKeyedScoped<IStore<ImportantDate>, RedisImportantDateStore>("redis");
        services.AddKeyedScoped<IStore<TaxBracket>, RedisTaxBracketStore>("redis");
        services.AddKeyedScoped<IStore<TaxRebate>, RedisTaxRebateStore>("redis");
    }
}