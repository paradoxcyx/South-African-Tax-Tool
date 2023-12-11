namespace southafricantaxtool.API.Models;

public class RetrieveTaxOutput
{
    public decimal AnnualTax { get; set; }
    public decimal MonthlyTax { get; set; }
    public decimal MonthlyNett { get; set; }
    public decimal AnnualNett { get; set; }
}