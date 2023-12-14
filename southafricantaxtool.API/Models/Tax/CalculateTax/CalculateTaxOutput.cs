namespace southafricantaxtool.API.Models.Tax.CalculateTax;

public class CalculateTaxOutput
{
    public decimal AnnualTax { get; set; }
    public decimal MonthlyTax { get; set; }
    public decimal MonthlyNett { get; set; }
    public decimal AnnualNett { get; set; }
    public string? Rule { get; set; }
    public List<string>? FormulaSteps { get; set; }
}