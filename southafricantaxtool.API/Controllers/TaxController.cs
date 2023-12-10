using Microsoft.AspNetCore.Mvc;
using southafricantaxtool.API.Models.RetrieveTaxData;
using southafricantaxtool.Shared;
using southafricantaxtool.Shared.Models;

namespace southafricantaxtool.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxController(ILogger<TaxController> logger) : ControllerBase
    {
        private readonly ILogger<TaxController> _logger = logger;

        [HttpPost("RetrieveTaxData")]
        public async Task<IActionResult> RetrieveTaxData([FromBody] RetrieveTaxDataInput input)
        {
            var taxData = await TaxScraper.RetrieveTaxData();

            var annualIncome = input.IsMonthly ? input.Income * 12 : input.Income;

            var taxBracket = taxData.Item1.FirstOrDefault(x =>
                x.Start?.Year is { } startYear &&
                x.End?.Year is { } endYear &&
                startYear <= input.TaxYear && endYear >= input.TaxYear);

            if (taxBracket == null)
            {
                return BadRequest($"Cannot find tax bracket for year: {input.TaxYear}");
            }

            var bracket = taxBracket.Brackets.FirstOrDefault(x =>
                (x.IncomeFrom <= annualIncome && x.IncomeTo >= annualIncome) ||
                (x.IncomeFrom <= annualIncome && !x.IncomeTo.HasValue));

            if (bracket == null)
            {
                return BadRequest($"Cannot find tax bracket for your income");
            }

            TaxRebate? taxRebate;

            switch (input.Age)
            {
                case >= 0 and <= 65:
                    taxRebate = taxData.Item2.FirstOrDefault(x => x.TaxRebateType == Shared.Enums.TaxRebateEnum.Primary);
                    break;
                case > 65 and <= 75:
                    taxRebate = taxData.Item2.FirstOrDefault(x => x.TaxRebateType == Shared.Enums.TaxRebateEnum.Secondary);
                    break;
                case > 75:
                    taxRebate = taxData.Item2.FirstOrDefault(x => x.TaxRebateType == Shared.Enums.TaxRebateEnum.Tertiary);
                    break;
                default:
                    return BadRequest("Invalid age");
            }

            var rebate = taxRebate!.Rebates.FirstOrDefault(x => x.Year == input.TaxYear);


            var tax = bracket.Rule.BaseAmount.HasValue
                ? bracket.Rule.BaseAmount.Value + (annualIncome - bracket.IncomeFrom) * bracket.Rule.Percentage / 100
                : Math.Round(annualIncome * ((decimal)bracket.Rule.Percentage / 100), 2);

            var monthlyTax = Math.Round((tax-rebate!.Amount) / 12, 2);
            var monthlyNett = Math.Round(input.IsMonthly ? input.Income - monthlyTax : (input.Income / 12) - monthlyTax, 2);

            var annualNett = annualIncome - (tax-rebate!.Amount);

            var rule = bracket.Rule.BaseAmount.HasValue ? $"{bracket.Rule.BaseAmount.Value:F2} + {bracket.Rule.Percentage}% of taxable income above {bracket.IncomeFrom:F2}" : $"{bracket.Rule.Percentage}% of Taxable Income";
            
            var output = new RetrieveTaxDataOutput
            {
                AnnualTax = tax,
                MonthlyTax = monthlyTax,
                MonthlyNett = monthlyNett,
                AnnualNett = annualNett,
                Rule = rule,
                FormulaSteps = BuildFormulaSteps(bracket, annualIncome, tax, rebate!.Amount)
            };

            return Ok(output);
        }


        private static List<string> BuildFormulaSteps(Bracket bracket, decimal income, decimal tax, decimal rebate)
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
}
