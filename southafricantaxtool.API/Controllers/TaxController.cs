using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using southafricantaxtool.API.Models.RetrieveTaxableAmount;
using southafricantaxtool.Shared;
using southafricantaxtool.Shared.Models;

namespace southafricantaxtool.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxController : ControllerBase
    {
        private readonly ILogger<TaxController> _logger;

        public TaxController(ILogger<TaxController> logger)
        {
            _logger = logger;
        }

        [HttpPost("RetrieveTaxableAmount")]
        public async Task<IActionResult> RetrieveTaxableAmount([FromBody] RetrieveTaxableAmountInput input)
        {
            var taxBrackets = await TaxScraper.RetrieveTaxBrackets();

            decimal annualIncome = input.IsMonthly ? input.Income * 12 : input.Income;

            var taxBracket = taxBrackets.FirstOrDefault(x =>
                x.Start?.Year is { } startYear &&
                x.End?.Year is { } endYear &&
                startYear <= input.Year && endYear >= input.Year);

            if (taxBracket == null)
            {
                return BadRequest($"Cannot find tax bracket for year: {input.Year}");
            }

            var bracket = taxBracket.Brackets.FirstOrDefault(x =>
                (x.IncomeFrom <= annualIncome && x.IncomeTo >= annualIncome) ||
                (x.IncomeFrom <= annualIncome && !x.IncomeTo.HasValue));

            if (bracket == null)
            {
                return BadRequest($"Cannot find tax bracket for your income");
            }

            var tax = bracket.Rule.BaseAmount.HasValue
                ? bracket.Rule.BaseAmount.Value + (annualIncome - bracket.IncomeFrom) * bracket.Rule.Percentage / 100
                : Math.Round(annualIncome * ((decimal)bracket.Rule.Percentage / 100), 2);

            var monthlyTax = tax / 12;
            var monthlyNett = input.IsMonthly ? input.Income - monthlyTax : (input.Income / 12) - monthlyTax;

            var annualNett = annualIncome - tax;

            var output = new RetrieveTaxableAmountOutput
            {
                AnnualTax = tax,
                MonthlyTax = monthlyTax,
                MonthlyNett = monthlyNett,
                AnnualNett = annualNett,
                FormulaSteps = BuildFormulaSteps(bracket, annualIncome, tax)
            };

            return Ok(output);
        }


        private List<string> BuildFormulaSteps(Bracket bracket, decimal income, decimal tax)
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

            steps.Add($"ANNUAL TAX = {tax:F2}");
            steps.Add($"MONTHLY TAX = {tax / 12:F2}");

            return steps;
        }

    }
}
