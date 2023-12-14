using HtmlAgilityPack;
using System.Text;
using southafricantaxtool.Scraper.Models;
using southafricantaxtool.Scraper.Utilities;

namespace southafricantaxtool.Scraper
{ 
    public class Scraper
    {
        private const string SarsTaxBracketIndividualUrl = "https://www.sars.gov.za/tax-rates/income-tax/rates-of-tax-for-individuals/";
        
        //IMPORTANT DATES - https://www.sars.gov.za/important-dates-4/
        //TURNOVER TAX (BUSINESS) - https://www.sars.gov.za/important-dates-4/
        
        public static async Task<SarsData> ScrapeSarsData()
        {
            var taxData = await RetrieveTaxData();

            var sarsData = new SarsData
            {
                TaxData = taxData
            };

            return sarsData;
        }
        public static async Task<TaxData> RetrieveTaxData()
        {
            var document = await ScrapeContent(SarsTaxBracketIndividualUrl) ?? throw new InvalidDataException("Unable to scrape this URL");
            
            var taxBracketYearNodes = document.DocumentNode.SelectNodes("//h2[strong[contains(., 'tax year')]]");
            var taxBracketNodes = document.DocumentNode.SelectNodes("//table[contains(@class, 'ms-rteTable') and .//th[contains(., 'Taxable income')]]");

            var taxRebateNode = document.DocumentNode.SelectSingleNode("//table[contains(@class, 'ms-rteTable') and .//th[contains(., 'Tax Rebate​​')]]");

            var taxRebates = ExtractTaxRebates(taxRebateNode);

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


            var taxData = new TaxData
            {
                TaxBrackets = taxBrackets,
                TaxRebates = taxRebates
            };
            return taxData;
        }

        public static async Task<List<TaxBracket>> RetrieveTaxBrackets()
        {
            var document = await ScrapeContent(SarsTaxBracketIndividualUrl) ?? throw new InvalidDataException("Unable to scrape this URL");
            
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

        public static async Task<List<TaxRebate>> RetrieveTaxRebates()
        {
            var document = await ScrapeContent(SarsTaxBracketIndividualUrl) ?? throw new InvalidDataException("Unable to scrape this URL");
            
            var taxRebateNode = document.DocumentNode.SelectSingleNode("//table[contains(@class, 'ms-rteTable') and .//th[contains(., 'Tax Rebate​​')]]");

            var taxRebates = ExtractTaxRebates(taxRebateNode);

            return taxRebates;
        }
        /// <summary>
        /// Scraping content of web url
        /// </summary>
        /// <param name="url">url to scrape</param>
        /// <returns>Scraped HTML Document</returns>
        private static async Task<HtmlDocument?> ScrapeContent(string url)
        {
            var web = new HtmlWeb();
            web.AutoDetectEncoding = false;
            web.OverrideEncoding = Encoding.UTF8;

            var document = await web.LoadFromWebAsync(url);
            return document;
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

        /// <summary>
        /// Extracting all tax rebates
        /// </summary>
        /// <param name="table">Html table node</param>
        /// <returns>A list of tax rebates</returns>
        private static List<TaxRebate> ExtractTaxRebates(HtmlNode table)
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
}
