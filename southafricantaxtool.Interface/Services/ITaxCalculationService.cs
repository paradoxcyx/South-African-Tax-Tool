using southafricantaxtool.Interface.Models;

namespace southafricantaxtool.BL.TaxCalculation;

public interface ITaxCalculationService
{
    /// <summary>
    /// Calculate annual tax
    /// </summary>
    /// <param name="bracket">The bracket for which to calculate tax</param>
    /// <param name="annualIncome">The annual income</param>
    /// <returns>Calculated annual tax</returns>
    public decimal CalculateAnnualTax(Bracket bracket, decimal annualIncome);

    /// <summary>
    /// Calculate monthly tax
    /// </summary>
    /// <param name="annualTax">The annual tax</param>
    /// <param name="rebateAmount">The rebate amount</param>
    /// <returns>Calculated monthly tax</returns>
    public decimal CalculateMonthlyTax(decimal annualTax, decimal rebateAmount);

    /// <summary>
    /// Calculate monthly nett
    /// </summary>
    /// <param name="monthlyIncome">The monthly income</param>
    /// <param name="monthlyTax">The monthly tax</param>
    /// <returns>Calculated monthly nett</returns>
    public decimal CalculateMonthlyNett(decimal monthlyIncome, decimal monthlyTax);

    /// <summary>
    /// Calculate annual nett
    /// </summary>
    /// <param name="annualIncome">The annual income</param>
    /// <param name="annualTax">The annual tax</param>
    /// <param name="rebateAmount">The rebate amount</param>
    /// <returns>Calculated annual nett</returns>
    public decimal CalculateAnnualNett(decimal annualIncome, decimal annualTax, decimal rebateAmount);

    /// <summary>
    /// Retrieving the tax rule description
    /// </summary>
    /// <param name="bracket">The bracket in which the rule lies</param>
    /// <returns>The rule description</returns>
    public string GetTaxRuleDescription(Bracket bracket);

    /// <summary>
    /// Build formula steps for the specified bracket
    /// </summary>
    /// <param name="bracket">The bracket</param>
    /// <param name="income">The annual income</param>
    /// <param name="tax">The annual tax</param>
    /// <param name="rebate">The rebate amount</param>
    /// <returns>List of formula steps</returns>
    public List<string> BuildFormulaSteps(Bracket bracket, decimal income, decimal tax, decimal rebate);
}