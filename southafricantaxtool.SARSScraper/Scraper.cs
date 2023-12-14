using HtmlAgilityPack;
using System.Text;

namespace southafricantaxtool.SARSScraper
{ 
    public abstract class Scraper
    {
        /// <summary>
        /// URL to scrape
        /// </summary>
        protected abstract string Url { get; }

        /// <summary>
        /// Scraping content of web url
        /// </summary>
        /// <returns>Scraped HTML Document</returns>
        protected async Task<HtmlDocument?> ScrapeContent()
        {
            var web = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.UTF8
            };

            var document = await web.LoadFromWebAsync(Url);
            return document;
        }
    }
}
