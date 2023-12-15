using System.Text.Json;
using System.Web;
using HtmlAgilityPack;
using southafricantaxtool.Interface.Models;

namespace southafricantaxtool.SARSScraper;

public class ImportantDateScraper : Scraper
{
    protected override string Url => "https://www.sars.gov.za/important-dates-4/";

    public async Task<List<ImportantDate>> RetrieveImportantDates()
    {
        var document = await ScrapeContent() ?? throw new InvalidDataException("Unable to scrape this URL");

        var dates = ExtractImportantDates(document);

        return dates;
    }

    private List<ImportantDate> ExtractImportantDates(HtmlDocument htmlDoc)
    {
        // Select the target element by ID
        string elementId = "eael-event-calendar-ae6cf43";
        HtmlNode targetElement = htmlDoc.GetElementbyId(elementId);

        // Get the value of the data-events attribute
        var dataEventsAttribute = targetElement.GetAttributeValue("data-events", "");

        var decodedImportantDates = HttpUtility.HtmlDecode(dataEventsAttribute);

        decodedImportantDates = decodedImportantDates.Replace("\\t", string.Empty);
        // Deserialize JSON string
        var dates = JsonSerializer.Deserialize<List<ImportantDate>>(decodedImportantDates);

        return dates;
        
    }
}