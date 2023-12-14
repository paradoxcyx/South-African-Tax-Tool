using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.BL.Services.Tax;

public interface ITaxService
{
    public Task<TaxData> GetTaxDataAsync();
}