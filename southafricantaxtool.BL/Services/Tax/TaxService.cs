using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using southafricantaxtool.Scraper;
using southafricantaxtool.Scraper.Enums;
using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.BL.Services.Tax;

public class TaxService(IDistributedCache cache) : ITaxService
{
    public async Task<TaxData> GetTaxDataAsync()
    {
        TaxData? taxData;

        var s = await cache.GetAsync("taxdata");
        if (s == null)
        {
            taxData = await TaxScraper.RetrieveTaxData();
            await cache.SetAsync("taxdata", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taxData)), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            });
        }
        else
        {
            var json = Encoding.UTF8.GetString(s);
            taxData = JsonConvert.DeserializeObject<TaxData>(json);
        }

        if (taxData == null)
            throw new InvalidOperationException("Unable to retrieve tax data");
            
        return taxData;
    }
}