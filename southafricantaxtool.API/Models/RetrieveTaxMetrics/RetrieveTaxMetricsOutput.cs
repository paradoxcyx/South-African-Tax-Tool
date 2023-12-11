namespace southafricantaxtool.API.Models.RetrieveTaxMetrics;

public class RetrieveTaxMetricsOutput : RetrieveTaxOutput
{
    public int Year { get; set; }
    public decimal? DifferenceFromPreviousYearPercentage { get; set; }
}