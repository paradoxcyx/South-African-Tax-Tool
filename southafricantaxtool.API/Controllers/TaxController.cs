using Microsoft.AspNetCore.Mvc;
using southafricantaxtool.API.Models;
using southafricantaxtool.API.Models.RetrieveTaxData;
using southafricantaxtool.API.Models.RetrieveTaxMetrics;
using southafricantaxtool.BL.Services.Tax;
using southafricantaxtool.BL.Services.TaxLookup;
using southafricantaxtool.BL.TaxCalculation;
using southafricantaxtool.DAL.Services;

namespace southafricantaxtool.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxController(ILogger<TaxController> logger, ITaxCalculationService taxCalculationService, ITaxLookupService taxLookupService, ITaxService taxService, MdbTaxBracketService mdbTaxBracketService, MdbTaxRebateService mdbTaxRebateService) : ControllerBase
    {
        [HttpPost("RetrieveTaxData")]
        public async Task<IActionResult> RetrieveTaxData([FromBody] RetrieveTaxDataInput input)
        {
            try
            {
                //var taxData = await taxService.GetTaxDataAsync();
                var taxBrackets = await mdbTaxBracketService.GetAsync();
                var taxRebates = await mdbTaxRebateService.GetAsync();
                
                var monthlyIncome = input.IsMonthly ? input.Income : input.Income / 12;
                var annualIncome = input.IsMonthly ? input.Income * 12 : input.Income;

                var taxBracket = taxLookupService.FindTaxBracketForYear(taxBrackets, input.TaxYear);
                var bracket = taxLookupService.FindTaxBracketForIncome(taxBracket.Brackets, annualIncome);
                var rebate = taxLookupService.FindTaxRebateForAgeAndYear(taxRebates, input.Age, input.TaxYear);

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
            try
            {
                var taxBrackets = await mdbTaxBracketService.GetAsync();
                var taxRebates = await mdbTaxRebateService.GetAsync();

                var monthlyIncome = input.IsMonthly ? input.Income : input.Income / 12;
                var annualIncome = input.IsMonthly ? input.Income * 12 : input.Income;

                var years = taxBrackets
                    .Where(x => x.End.HasValue)
                    .Select(x => x.End!.Value.Year)
                    .OrderByDescending(o => o)
                    .ToList();

                var metrics = new List<RetrieveTaxMetricsOutput>();
                RetrieveTaxMetricsOutput? previousMetric = null;

                foreach (var year in years)
                {
                    var taxBracket = taxLookupService.FindTaxBracketForYear(taxBrackets, year);
                    var bracket = taxLookupService.FindTaxBracketForIncome(taxBracket.Brackets, annualIncome);
                    var rebate = taxLookupService.FindTaxRebateForAgeAndYear(taxRebates, input.Age, year);

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
                        metric.DifferenceFromPreviousYearPercentage =
                            Math.Round((previousMetric.AnnualTax - annualTax) / annualTax * 100, 2);
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
            catch (InvalidOperationException op)
            {
                return BadRequest(new GenericResponseModel<List<RetrieveTaxMetricsOutput>>
                {
                    Success = false,
                    Message = op.Message
                });
            }
            
        }
    }
}
