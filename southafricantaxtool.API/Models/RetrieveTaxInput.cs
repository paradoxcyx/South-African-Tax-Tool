namespace southafricantaxtool.API.Models;

public class RetrieveTaxInput
{
    public int Age { get; set; }
    public decimal Income { get; set; }
    public bool IsMonthly { get; set; }
}