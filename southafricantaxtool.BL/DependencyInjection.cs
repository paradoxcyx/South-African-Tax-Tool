using Microsoft.Extensions.DependencyInjection;
using southafricantaxtool.BL.Services.Tax;
using southafricantaxtool.BL.Services.TaxLookup;
using southafricantaxtool.BL.TaxCalculation;

namespace southafricantaxtool.BL;

public static class DependencyInjection
{
    /// <summary>
    /// Register scoped services for business logic.
    /// </summary>
    /// <param name="services"></param>
    public static void AddBusinessLogicServices(this IServiceCollection services)
    {
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<ITaxCalculationService, TaxCalculationService>();
        services.AddScoped<ITaxLookupService, TaxLookupService>();
    }
}