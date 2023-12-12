using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using southafricantaxtool.API.Models;
using southafricantaxtool.API.Models.RetrieveTaxData;
using southafricantaxtool.API.Models.RetrieveTaxMetrics;
using southafricantaxtool.BL.Services.Tax;
using southafricantaxtool.BL.Services.TaxLookup;
using southafricantaxtool.BL.TaxCalculation;
using southafricantaxtool.Scraper;
using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxController(ILogger<TaxController> logger, ITaxCalculationService taxCalculationService, ITaxLookupService taxLookupService, ITaxService taxService) : ControllerBase
    {

        
        
        [HttpPost("RetrieveTaxData")]
        public async Task<IActionResult> RetrieveTaxData([FromBody] RetrieveTaxDataInput input)
        {
            try
            {
                var taxData = await taxService.GetTaxDataAsync();
                
                var monthlyIncome = input.IsMonthly ? input.Income : input.Income / 12;
                var annualIncome = input.IsMonthly ? input.Income * 12 : input.Income;

                var taxBracket = taxLookupService.FindTaxBracketForYear(taxData.TaxBrackets, input.TaxYear);
                var bracket = taxLookupService.FindTaxBracketForIncome(taxBracket.Brackets, annualIncome);
                var rebate = taxLookupService.FindTaxRebateForAgeAndYear(taxData.TaxRebates, input.Age, input.TaxYear);

                var annualTax = taxCalculationService.CalculateAnnualTax(bracket, annualIncome);
                var monthlyTax = taxCalculationService.CalculateMonthlyTax(annualTax, rebate.Amount);
                var monthlyNett = taxCalculationService.CalculateMonthlyNett(monthlyIncome, monthlyTax);
                var annualNett = taxCalculationService.CalculateAnnualNett(annualIncome, annualTax, rebate.Amount);
                var rule = taxCalculationService.GetTaxRuleDescription(bracket);

                var output = new RetrieveTaxDataOutput
                {
                    AnnualTax = annualTax,
                    MonthlyTax = monthlyTax,
                    MonthlyNett = monthlyNett,
                    AnnualNett = annualNett,
                    Rule = rule,
                    FormulaSteps = taxCalculationService.BuildFormulaSteps(bracket, annualIncome, annualTax, rebate!.Amount)
                };

                var response = new GenericResponseModel<RetrieveTaxDataOutput>
                {
                    Success = true,
                    Message = "Tax data retrieved successfully",
                    Data = output
                };

                return Ok(response);
            }
            catch (InvalidOperationException op)
            {
                logger.LogError(op.Message);

                return BadRequest(new GenericResponseModel<RetrieveTaxDataOutput>
                {
                    Success = false,
                    Message = op.Message
                });
            }
        }

        [HttpPost("RetrieveTaxMetrics")]
        public async Task<IActionResult> RetrieveTaxMetrics([FromBody] RetrieveTaxMetricsInput input)
        {
            var taxData = await taxService.GetTaxDataAsync();
    
            var monthlyIncome = input.IsMonthly ? input.Income : input.Income / 12;
            var annualIncome = input.IsMonthly ? input.Income * 12 : input.Income;

            var years = taxData.TaxBrackets
                .Where(x => x.End.HasValue)
                .Select(x => x.End!.Value.Year)
                .OrderByDescending(o => o)
                .ToList();

            var metrics = new List<RetrieveTaxMetricsOutput>();
            RetrieveTaxMetricsOutput? previousMetric = null;

            foreach (var year in years)
            {
                var taxBracket = taxLookupService.FindTaxBracketForYear(taxData.TaxBrackets, year);
                var bracket = taxLookupService.FindTaxBracketForIncome(taxBracket.Brackets, annualIncome);
                var rebate = taxLookupService.FindTaxRebateForAgeAndYear(taxData.TaxRebates, input.Age, year);

                var annualTax = taxCalculationService.CalculateAnnualTax(bracket, annualIncome);
                var monthlyTax = taxCalculationService.CalculateMonthlyTax(annualTax, rebate.Amount);
                var monthlyNett = taxCalculationService.CalculateMonthlyNett(monthlyIncome, monthlyTax);
                var annualNett = taxCalculationService.CalculateAnnualNett(annualIncome, annualTax, rebate.Amount);

                var metric = new RetrieveTaxMetricsOutput
                {
                    Year = year,
                    AnnualTax = annualTax,
                    MonthlyTax = monthlyTax,
                    MonthlyNett = monthlyNett,
                    AnnualNett = annualNett,
                };

                if (previousMetric != null)
                {
                    metric.DifferenceFromPreviousYearPercentage = Math.Round((previousMetric.AnnualTax - annualTax) / annualTax * 100, 2);
                }

                metrics.Add(metric);
                previousMetric = metric;
            }

            var response = new GenericResponseModel<List<RetrieveTaxMetricsOutput>>
            {
                Success = true,
                Message = "Tax metrics retrieved successfully",
                Data = metrics
            };

            return Ok(response);
        }

    }
}
