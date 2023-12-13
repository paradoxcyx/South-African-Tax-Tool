using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using southafricantaxtool.Scraper;
using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.BL.Services.Tax;

public class TaxService(IDistributedCache cache) : ITaxService
{
    public async Task<TaxData> GetTaxDataAsync()
    {
        var cachedData = await cache.GetAsync("taxdata");
        
        if (cachedData != null)
        {
            var json = Encoding.UTF8.GetString(cachedData);
            return JsonConvert.DeserializeObject<TaxData>(json);
        }

        var taxData = await Scraper.Scraper.RetrieveTaxData();

        if (taxData == null)
        {
            throw new InvalidOperationException("Unable to retrieve tax data");
        }

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
        };

        await cache.SetAsync("taxdata", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taxData)), cacheOptions);

        return taxData;
    }

}