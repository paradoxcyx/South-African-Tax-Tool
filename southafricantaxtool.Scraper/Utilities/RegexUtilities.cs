namespace southafricantaxtool.Scraper.Utilities;

internal static class RegexUtilities
{
    /// <summary>
    /// Extracting a decimal value from scraped html
    /// </summary>
    /// <param name="input">scraped html</param>
    /// <returns>decimal value</returns>
    public static decimal ExtractDecimalValue(string input)
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
    public static int ExtractNumber(string input)
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
    
    /// <summary>
    /// Number parser which removes white space and parses text to decimal
    /// </summary>
    /// <param name="numberString">The unparsed text</param>
    /// <returns>The parsed number</returns>
    public static decimal ParseNumber(string numberString)
    {
        var text = RegexPatterns.ReplaceWhitespacesRegex().Replace(numberString,"");

        // Remove any spaces from the number string and parse as an integer
        return decimal.Parse(text);
    }
    
    /// <summary>
    /// Extracting the date range from html inner text
    /// </summary>
    /// <param name="text">Html inner text</param>
    /// <returns>A tuple which represets the start and end date</returns>
    public static Tuple<DateTime, DateTime>? ExtractDates(string text)
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
        /// Extracting the Income Range (From and To) from html inner text
        /// </summary>
        /// <param name="inputText">The HTML inner text</param>
        /// <returns>a Tuple that represents the Income From and To</returns>
        public static Tuple<decimal, decimal?>? ExtractIncomeRange(string inputText)
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
}