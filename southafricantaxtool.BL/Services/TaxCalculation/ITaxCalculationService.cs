using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.BL.TaxCalculation;

public interface ITaxCalculationService
{
    public decimal CalculateAnnualTax(Bracket bracket, decimal annualIncome);

    public decimal CalculateMonthlyTax(decimal annualTax, decimal rebateAmount);

    public decimal CalculateMonthlyNett(decimal monthlyIncome, decimal monthlyTax);

    public decimal CalculateAnnualNett(decimal annualIncome, decimal annualTax, decimal rebateAmount);

    public string GetTaxRuleDescription(Bracket bracket);

    public List<string> BuildFormulaSteps(Bracket bracket, decimal income, decimal tax, decimal rebate);
}