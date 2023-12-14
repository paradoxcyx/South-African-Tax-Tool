using HtmlAgilityPack;
using southafricantaxtool.SARSScraper.Models;
using southafricantaxtool.SARSScraper.Utilities;

namespace southafricantaxtool.SARSScraper;

public class TaxRebateScraper : Scraper
{
    protected override string Url => "https://www.sars.gov.za/tax-rates/income-tax/rates-of-tax-for-individuals/";
    
    public async Task<List<TaxRebate>> RetrieveTaxRebates()
    {
        var document = await ScrapeContent() ?? throw new InvalidDataException("Unable to scrape this URL");
            
        var taxRebateNode = document.DocumentNode.SelectSingleNode("//table[contains(@class, 'ms-rteTable') and .//th[contains(., 'Tax Rebate​​')]]");

        var taxRebates = ExtractTaxRebates(taxRebateNode);

        return taxRebates;
    }

    /// <summary>
    /// Extracting all tax rebates
    /// </summary>
    /// <param name="table">Html table node</param>
    /// <returns>A list of tax rebates</returns>
    private List<TaxRebate> ExtractTaxRebates(HtmlNode table)
    {
            
        var rows = table.SelectNodes(".//tr[@class='ms-rteTableEvenRow-default' or @class='ms-rteTableOddRow-default']");
        var dataRows = table.SelectNodes(".//tr[@class='ms-rteTableEvenRow-default' or @class='ms-rteTableOddRow-default']").Skip(1).ToList();

        // Extract headers (years)
        var headerRow = rows[0];
        var headers = headerRow.Elements("td").Skip(1).Select(th => th.InnerText.Trim()).ToList();

        var taxRebates = new List<TaxRebate>
        {
            new() {
                TaxRebateType = Enums.TaxRebateEnum.Primary
            },
            new()
            {
                TaxRebateType = Enums.TaxRebateEnum.Secondary
            },
            new()
            {
                TaxRebateType = Enums.TaxRebateEnum.Tertiary
            },
        };

        for (var x=0; x<dataRows.Count; x++)
        {
            var rowData = dataRows[x].Elements("td").Skip(1).ToList();
            taxRebates[x].Rebates = [];

            for (var y = 0; y<rowData.Count; y++)
            {
                taxRebates[x].Rebates.Add(new Rebate { Year = RegexUtilities.ExtractNumber(headers[y]), Amount = RegexUtilities.ExtractDecimalValue(rowData[y].Element("span").InnerHtml) });
            }
        }

        return taxRebates;
           
    }
    
}