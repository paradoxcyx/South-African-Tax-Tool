namespace southafricantaxtool.API.Models.Tax.CalculateTax;

public class CalculateTaxInput
{
    public int TaxYear { get; set; }
    public int Age { get; set; }
    public decimal Income { get; set; }
    public bool IsMonthly { get; set; }
}