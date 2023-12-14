namespace southafricantaxtool.API.Models.Tax.CalculateTaxMetrics;

public class CalculateTaxMetricsOutput
{
    public decimal AnnualTax { get; set; }
    public decimal MonthlyTax { get; set; }
    public decimal MonthlyNett { get; set; }
    public decimal AnnualNett { get; set; }
    public int Year { get; set; }
    public decimal? DifferenceFromPreviousYearPercentage { get; set; }
}