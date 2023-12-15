using southafricantaxtool.Interface.Enums;
using southafricantaxtool.Interface.Models;

namespace southafricantaxtool.BL.TaxCalculation;

public class TaxCalculationService : ITaxCalculationService
{
    public decimal CalculateAnnualTax(Bracket bracket, decimal annualIncome)
    {
        return bracket.Rule.BaseAmount.HasValue
            ? bracket.Rule.BaseAmount.Value + (annualIncome - bracket.IncomeFrom) * bracket.Rule.Percentage / 100
            : Math.Round(annualIncome * ((decimal)bracket.Rule.Percentage / 100), 2);
    }

    public decimal CalculateMonthlyTax(decimal annualTax, decimal rebateAmount)
    {
        return Math.Round((annualTax - rebateAmount) / 12, 2);
    }

    public decimal CalculateMonthlyNett(decimal monthlyIncome, decimal monthlyTax)
    {
        return Math.Round(monthlyIncome - monthlyTax, 2);
    }

    public decimal CalculateAnnualNett(decimal annualIncome, decimal annualTax, decimal rebateAmount)
    {
        return annualIncome - (annualTax - rebateAmount);
    }

    public string GetTaxRuleDescription(Bracket bracket)
    {
        return bracket.Rule.BaseAmount.HasValue
            ? $"{bracket.Rule.BaseAmount.Value:F2} + {bracket.Rule.Percentage}% of taxable income above {bracket.IncomeFrom:F2}"
            : $"{bracket.Rule.Percentage}% of Taxable Income";
    }
    
    public List<string> BuildFormulaSteps(Bracket bracket, decimal income, decimal tax, decimal rebate)
    {
        var steps = new List<string>();

        if (bracket.Rule.BaseAmount.HasValue)
        {
            var taxableIncomeAboveThreshold = income - bracket.IncomeFrom;
            var percentageTax = taxableIncomeAboveThreshold * bracket.Rule.Percentage / 100;

            steps.Add($"TAX = {bracket.Rule.BaseAmount.Value:F2} + {bracket.Rule.Percentage}% X ({income:F2} - {bracket.IncomeFrom:F2})");
            steps.Add($"TAX = {bracket.Rule.BaseAmount.Value:F2} + {bracket.Rule.Percentage}% X {taxableIncomeAboveThreshold:F2}");
            steps.Add($"TAX = {bracket.Rule.BaseAmount.Value:F2} + {percentageTax:F2}");
        }
        else
        {
            steps.Add($"TAX = {income:F2} X {bracket.Rule.Percentage}%");
        }

        steps.Add($"TAX = {tax:F2} - {rebate:F2} (Rebate)");

        var finalTax = tax - rebate;

        steps.Add($"ANNUAL TAX = {finalTax:F2}");
        steps.Add($"MONTHLY TAX = {finalTax / 12:F2}");

        return steps;
    }
}