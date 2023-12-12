using HtmlAgilityPack;
using System.Text;
using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.Scraper
{ 
    public class TaxScraper
    {
        private const string SarsTaxBracketIndividualUrl = "https://www.sars.gov.za/tax-rates/income-tax/rates-of-tax-for-individuals/";

        public static async Task<TaxData> RetrieveTaxData()
        {
            var document = await ScrapeContent() ?? throw new InvalidDataException("Unable to scrape SARS tax brackets");

            var taxBracketYearNodes = document.DocumentNode.SelectNodes("//h2[strong[contains(., 'tax year')]]");
            var taxBracketNodes = document.DocumentNode.SelectNodes("//table[contains(@class, 'ms-rteTable') and .//th[contains(., 'Taxable income')]]");

            var taxRebateNode = document.DocumentNode.SelectSingleNode("//table[contains(@class, 'ms-rteTable') and .//th[contains(., 'Tax Rebate​​')]]");

            var taxRebates = ExtractTaxRebates(taxRebateNode);

            List<TaxBracket> taxBrackets = [];

            for (var x = 0; x < taxBracketYearNodes.Count; x++)
            {
                var taxBracket = new TaxBracket();

                var datesTuple = ExtractDates(taxBracketYearNodes[x].InnerHtml);

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


        /// <summary>
        /// Scraping the content of SARS' tax brackets (Year: 2017 - Present)
        /// </summary>
        /// <returns>Scraped HTML Document</returns>
        private static async Task<HtmlDocument?> ScrapeContent()
        {
            var web = new HtmlWeb();
            web.AutoDetectEncoding = false;
            web.OverrideEncoding = Encoding.UTF8;

            var document = await web.LoadFromWebAsync(SarsTaxBracketIndividualUrl);
            return document;
        }

        /// <summary>
        /// Extracting the date range from html inner text
        /// </summary>
        /// <param name="text">Html inner text</param>
        /// <returns>A tuple which represets the start and end date</returns>
        private static Tuple<DateTime, DateTime>? ExtractDates(string text)
        {
            // Use Regex to match the pattern in the input text
            var match = RegexPatterns.DatesExtractionRegex().Match(text);

            if (!match.Success) return null;
            
            // Retrieve matched groups
            var startMonth = match.Groups["startMonth"].Value;
            var startYear = int.Parse(match.Groups["startYear"].Value);
            var endDay = int.Parse(match.Groups["endDay"].Value);
            var endMonth = match.Groups["endMonth"].Value;
            var endYear = int.Parse(match.Groups["endYear"].Value);

            // Convert month names to month numbers
            DateTime startDateTime = new(startYear, DateTime.ParseExact(startMonth, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month, 1);
            DateTime endDateTime = new(endYear, DateTime.ParseExact(endMonth, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month, endDay);

            return Tuple.Create(startDateTime, endDateTime);

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


                var incomeRangeValues = ExtractIncomeRange(incomeRange);

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
        /// Extracting the Income Range (From and To) from html inner text
        /// </summary>
        /// <param name="inputText">The HTML inner text</param>
        /// <returns>a Tuple that represents the Income From and To</returns>
        private static Tuple<decimal, decimal?>? ExtractIncomeRange(string inputText)
        {
            try
            {
                if (inputText.Contains("and above", StringComparison.InvariantCultureIgnoreCase))
                {
                    inputText = inputText.Replace("and above", string.Empty).Replace(" ", string.Empty).Trim();

                    decimal.TryParse(inputText, out var rangeFrom);
                    decimal? rangeTo = null;

                    return Tuple.Create(rangeFrom, rangeTo);

                }

                //Cleaning
                inputText = RegexPatterns.ReplaceUnknownUnicodeRegex().Replace(inputText, "|").Trim();

                const string replacement = "|";

                var cleanedInputText = RegexPatterns.ReplaceAllHyphensAndDashesRegex()
                    .Replace(inputText, replacement)
                    .Replace(" ", string.Empty).Trim();
                
                // Match the input against the regex
                var match = RegexPatterns.ExtractIncomeRangeRegex()
                    .Match(cleanedInputText);

                if (match.Groups.Count == 1)
                {
                    Console.WriteLine(cleanedInputText);
                }

                var from = match.Groups[1].Value;
                var to = match.Groups[2].Success ? match.Groups[2].Value : null;

                var fromValue = decimal.Parse(from);
                decimal? toValue = !string.IsNullOrEmpty(to) ? decimal.Parse(to) : null;

                return Tuple.Create(fromValue, toValue);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {inputText} - {ex}");
                Console.ForegroundColor = ConsoleColor.White;

                return null;
            }

        }

        /// <summary>
        /// Number parser which removes white space and parses text to decimal
        /// </summary>
        /// <param name="numberString">The unparsed text</param>
        /// <returns>The parsed number</returns>
        private static decimal ParseNumber(string numberString)
        {
            var text = RegexPatterns.ReplaceWhitespacesRegex().Replace(numberString,"");

            // Remove any spaces from the number string and parse as an integer
            return decimal.Parse(text);
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
                baseAmount = match.Groups[1].Success ? ParseNumber(match.Groups[1].Value) : null;
                percentage = int.Parse(match.Groups[2].Value);
                threshold = match.Groups[3].Success ? ParseNumber(match.Groups[3].Value) : null;

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
                    taxRebates[x].Rebates.Add(new Rebate { Year = ExtractNumber(headers[y]), Amount = ExtractDecimalValue(rowData[y].Element("span").InnerHtml) });
                    
                }
            }

            return taxRebates;
           
        }

        /// <summary>
        /// Extracting a decimal value from scraped html
        /// </summary>
        /// <param name="input">scraped html</param>
        /// <returns>decimal value</returns>
        private static decimal ExtractDecimalValue(string input)
        {
            var text = RegexPatterns.ReplaceWhitespacesRegex().Replace(input, "");

            // Use regular expression to extract decimal value
            var match = RegexPatterns.ExtractDigitsRegex().Match(text);
            if (match.Success && decimal.TryParse(match.Value.Replace(",", ""), out decimal result))
            {
                return result;
            }
            return 0.0m;
        }

        /// <summary>
        /// Extracting a number value from scraped html
        /// </summary>
        /// <param name="input">scraped html</param>
        /// <returns>number value</returns>
        private static int ExtractNumber(string input)
        {
            var text = RegexPatterns.ReplaceWhitespacesRegex().Replace(input, "");

            // Use regular expression to extract decimal value
            var match = RegexPatterns.ExtractDigitsRegex().Match(text);
            if (match.Success && int.TryParse(match.Value, out var result))
            {
                return result;
            }
            return 0;
        }
    }
}
