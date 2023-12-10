using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;
using SARSTaxBracketScraper.Models;

namespace SARSTaxBracketScraper
{
    public class TaxScraper
    {
        private const string sarsTaxBracketIndividualUrl = "https://www.sars.gov.za/tax-rates/income-tax/rates-of-tax-for-individuals/";

        public static async Task<List<TaxBracket>> RetrieveTaxBrackets()
        {
            var document = await ScrapeContent() ?? throw new InvalidDataException("Unable to scrape SARS tax brackets");

            var taxBracketYearNodes = document.DocumentNode.SelectNodes("//h2[strong[contains(., 'tax year')]]");
            var taxBracketNodes = document.DocumentNode.SelectNodes("//table[contains(@class, 'ms-rteTable') and .//th[contains(., 'Taxable income')]]");

            List<TaxBracket> taxBrackets = [];

            for (int x = 0; x < taxBracketYearNodes.Count; x++)
            {
                var taxBracket = new TaxBracket();

                var datesTuple = ExtractDates(taxBracketYearNodes[x].InnerHtml);

                if (datesTuple != null)
                {
                    taxBracket.Start = datesTuple.Item1;
                    taxBracket.End = datesTuple.Item2;

                    if (taxBracket.End.Value.Year == 2016)
                    {
                        return taxBrackets;
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
        /// Scraping the content of SARS' tax brackets (Year: 2017 - Present)
        /// </summary>
        /// <returns>Scraped HTML Document</returns>
        private static async Task<HtmlDocument?> ScrapeContent()
        {
            var web = new HtmlWeb();
            web.AutoDetectEncoding = false;
            web.OverrideEncoding = Encoding.UTF8;

            var document = await web.LoadFromWebAsync(sarsTaxBracketIndividualUrl);
            return document;
        }

        /// <summary>
        /// Extracting the date range from html inner text
        /// </summary>
        /// <param name="text">Html inner text</param>
        /// <returns>A tuple which represets the start and end date</returns>
        private static Tuple<DateTime, DateTime>? ExtractDates(string text)
        {
            // Define a regular expression pattern to match the date components
            string pattern = @"(?<startMonth>\w+)\s(?<startYear>\d+)\s&#8211;\s(?<endDay>\d+)\s(?<endMonth>\w+)\s(?<endYear>\d+)";

            // Use Regex to match the pattern in the input text
            Match match = Regex.Match(text, pattern);

            if (match.Success)
            {
                // Retrieve matched groups
                string startMonth = match.Groups["startMonth"].Value;
                int startYear = int.Parse(match.Groups["startYear"].Value);
                int endDay = int.Parse(match.Groups["endDay"].Value);
                string endMonth = match.Groups["endMonth"].Value;
                int endYear = int.Parse(match.Groups["endYear"].Value);

                // Convert month names to month numbers
                DateTime startDateTime = new(startYear, DateTime.ParseExact(startMonth, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month, 1);
                DateTime endDateTime = new(endYear, DateTime.ParseExact(endMonth, "MMMM", System.Globalization.CultureInfo.InvariantCulture).Month, endDay);

                return Tuple.Create(startDateTime, endDateTime);
            }

            return null;

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

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    // Extract the columns (td elements) from each row
                    var columns = row.SelectNodes("td");

                    if (columns != null && columns.Count == 2)
                    {
                        var bracket = new Bracket();

                        // Extract the text content from the columns
                        string incomeRange = columns[0].InnerText.Trim();


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


                        string taxRate = columns[1].InnerText.Trim();

                        var bracketRule = ExtractTaxBracketRules(taxRate);

                        bracket.Rule = new BracketRule { BaseAmount = bracketRule.Item1, Percentage = bracketRule.Item2, Threshold = bracketRule.Item3 };

                        // Add the extracted data to the list
                        taxBrackets.Add(bracket);
                    }
                }
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
                if (inputText.Contains("and"))
                {
                    var s = "";
                }
                if (inputText.Contains("and above", StringComparison.InvariantCultureIgnoreCase))
                {
                    inputText = inputText.Replace("and above", string.Empty).Replace(" ", string.Empty).Trim();

                    decimal.TryParse(inputText, out decimal rangeFrom);
                    decimal? rangeTo = null;

                    return Tuple.Create(rangeFrom, rangeTo);

                }

                //Cleaning
                inputText = Regex.Replace(inputText, "&#8211;", "|").Trim();

                // Use a regular expression to match any dash character and replace with a period.
                string pattern2 = "[\\p{Pd}]";  // \p{Pd} matches any kind of dash
                string replacement = "|";
                Regex cleaningRegex = new Regex(pattern2);

                string cleanedInputText = cleaningRegex.Replace(inputText, replacement).Replace(" ", string.Empty).Trim();

                // Define the regex pattern
                string pattern = @"^[\s]*(\d+)[\s]*\|[\s]*(\d+)[\s]*$";


                // Create a Regex object
                Regex regex = new(pattern, RegexOptions.IgnorePatternWhitespace);

                // Match the input against the regex
                Match match = regex.Match(cleanedInputText);

                if (match.Groups.Count == 1)
                {
                    Console.WriteLine(cleanedInputText);
                }

                string from = match.Groups[1].Value;
                string? to = match.Groups[2].Success ? match.Groups[2].Value : null;

                decimal fromValue = decimal.Parse(from);
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
            // Remove any spaces from the number string and parse as an integer
            return decimal.Parse(numberString.Replace(" ", ""));
        }

        /// <summary>
        /// Extracting the rules for a specific tax bracket
        /// </summary>
        /// <param name="inputText">The rules in plain HTML inner text</param>
        /// <returns>A tuple that represents the rules</returns>
        private static Tuple<decimal?, int, decimal?> ExtractTaxBracketRules(string inputText)
        {
            // Define the regex pattern for extracting base amount, percentage, and threshold
            var pattern = @"(?:(\d+(?:\s*\d{3})*)\s*\+\s*)?(\d+)%\s*of\s*(?:taxable\s*income\s*above\s*(\d+(?:\s*\d{3})*)|$|each\s*R1)?";

            // Use regex to match the pattern
            var match = Regex.Match(inputText, pattern);

            // Extract values from the match
            decimal? baseAmount = match.Groups[1].Success ? ParseNumber(match.Groups[1].Value) : (int?)null;
            int percentage = int.Parse(match.Groups[2].Value);
            decimal? threshold = match.Groups[3].Success ? ParseNumber(match.Groups[3].Value) : (int?)null;

            return Tuple.Create(baseAmount, percentage, threshold);
        }
    }
}
