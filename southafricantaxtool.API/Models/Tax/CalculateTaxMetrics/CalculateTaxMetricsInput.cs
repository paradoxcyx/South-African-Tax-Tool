namespace southafricantaxtool.API.Models.Tax.CalculateTaxMetrics;

public class CalculateTaxMetricsInput
{
    public int Age { get; set; }
    public decimal Income { get; set; }
    public bool IsMonthly { get; set; }
}