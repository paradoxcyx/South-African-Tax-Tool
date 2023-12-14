using HtmlAgilityPack;
using southafricantaxtool.SARSScraper.Models;
using southafricantaxtool.SARSScraper.Utilities;

namespace southafricantaxtool.SARSScraper;

public class TaxBracketScraper : Scraper
{
    protected override string Url => "https://www.sars.gov.za/tax-rates/income-tax/rates-of-tax-for-individuals/";

    public async Task<List<TaxBracket>> RetrieveTaxBrackets()
    {
        var document = await ScrapeContent() ?? throw new InvalidDataException("Unable to scrape this URL");
            
        var taxBracketYearNodes = document.DocumentNode.SelectNodes("//h2[strong[contains(., 'tax year')]]");
        var taxBracketNodes = document.DocumentNode.SelectNodes("//table[contains(@class, 'ms-rteTable') and .//th[contains(., 'Taxable income')]]");
            
        List<TaxBracket> taxBrackets = [];

        for (var x = 0; x < taxBracketYearNodes.Count; x++)
        {
            var taxBracket = new TaxBracket();

            var datesTuple = RegexUtilities.ExtractDates(taxBracketYearNodes[x].InnerHtml);

            if (datesTuple != null)
            {
                taxBracket.Start = datesTuple.Item1;
                taxBracket.End = datesTuple.Item2;

                //YEAR LIMITATION 
                //TODO: Investigate scraping limitations when trying to scrape data before 2016
                if (taxBracket.End.Value.Year == 2016)
                {
                    break;
                }
            }

            var brackets = taxBracketNodes[x];

            if (brackets != null)
            {
                taxBracket.Brackets = ExtractTaxBrackets(brackets);
            }

            taxBrackets.Add(taxBracket);
        }

        return taxBrackets;

    }
    
    /// <summary>
    /// Extracting the tax bracket rows of the given year's table
    /// </summary>
    /// <param name="table">The HTML Table (Node)</param>
    /// <returns>List of brackets</returns>
    private static List<Bracket> ExtractTaxBrackets(HtmlNode table)
    {
        List<Bracket> taxBrackets = [];

        // Select all rows in the table except the header and footer rows
        var rows = table.SelectNodes(".//tr[contains(@class, 'ms-rteTable') and not(contains(@class, 'Header'))]");

        if (rows == null) return taxBrackets;
            
        foreach (var row in rows)
        {
            // Extract the columns (td elements) from each row
            var columns = row.SelectNodes("td");

            if (columns is not { Count: 2 }) continue;
                
            var bracket = new Bracket();

            // Extract the text content from the columns
            var incomeRange = columns[0].InnerText.Trim();


            var incomeRangeValues = RegexUtilities.ExtractIncomeRange(incomeRange);

            if (incomeRangeValues != null)
            {
                bracket.IncomeFrom = incomeRangeValues.Item1;
                bracket.IncomeTo = incomeRangeValues.Item2;
            }
            else
            {
                bracket.IncomeFrom = 0;
                bracket.IncomeTo = 0;
            }


            var taxRate = columns[1].InnerText.Trim();

            bracket.Rule = ExtractTaxBracketRules(taxRate);

            // Add the extracted data to the list
            taxBrackets.Add(bracket);
        }

        return taxBrackets;
    }
    
    /// <summary>
    /// Extracting the rules for a specific tax bracket
    /// </summary>
    /// <param name="inputText">The rules in plain HTML inner text</param>
    /// <returns>A tuple that represents the rules</returns>
    private static BracketRule ExtractTaxBracketRules(string inputText)
    {
        decimal? baseAmount = null;
        var percentage = 0;
        decimal? threshold = null;
            
        try
        {
            // Use regex to match the pattern
            var match = RegexPatterns.TaxBracketRuleExtractionRegex().Match(inputText);

            // Extract values from the match
            baseAmount = match.Groups[1].Success ? RegexUtilities.ParseNumber(match.Groups[1].Value) : null;
            percentage = int.Parse(match.Groups[2].Value);
            threshold = match.Groups[3].Success ? RegexUtilities.ParseNumber(match.Groups[3].Value) : null;

            return new BracketRule
            {
                BaseAmount = baseAmount,
                Percentage = percentage,
                Threshold = threshold
            };
        }
        catch (Exception e)
        {
            return new BracketRule
            {
                BaseAmount = baseAmount,
                Percentage = percentage,
                Threshold = threshold
            };
        }
            
    }
    
}