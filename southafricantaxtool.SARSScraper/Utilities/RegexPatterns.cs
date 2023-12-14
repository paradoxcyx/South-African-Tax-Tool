using System.Text.RegularExpressions;

namespace southafricantaxtool.SARSScraper.Utilities;

public static partial class RegexPatterns
{
    /// <summary>
    /// Define a regular expression pattern to match the date components
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"(?<startMonth>\w+)\s(?<startYear>\d+)\s&#8211;\s(?<endDay>\d+)\s(?<endMonth>\w+)\s(?<endYear>\d+)")]
    public static partial Regex DatesExtractionRegex();
    
    /// <summary>
    /// Define the regex pattern for extracting base amount, percentage, and threshold
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"(?:(\d+(?:\s*\d{3})*)\s*\+\s*)?(\d+)%\s*of\s*(?:taxable\s*income\s*above\s*(\d+(?:\s*\d{3})*)|$|each\s*R1)?")]
    public static partial Regex TaxBracketRuleExtractionRegex();
    
    /// <summary>
    /// Define the regex pattern for replacing white spaces
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\s+")]
    public static partial Regex ReplaceWhitespacesRegex();
    
    /// <summary>
    /// Define the regex pattern for extracting digits
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"[\d,]+")]
    public static partial Regex ExtractDigitsRegex();
    
    /// <summary>
    /// Define the regex pattern for replacing unknown unicode
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("&#8211;")]
    public static partial Regex ReplaceUnknownUnicodeRegex();
    
    /// <summary>
    /// Define the regex pattern for replacing all types of hyphens and dashes
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"[\p{Pd}]")]
    public static partial Regex ReplaceAllHyphensAndDashesRegex();
    
    /// <summary>
    /// Define the regex pattern for extracting the income range
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^[\s]*(\d+)[\s]*\|[\s]*(\d+)[\s]*$", RegexOptions.IgnorePatternWhitespace)]
    public static partial Regex ExtractIncomeRangeRegex();
}