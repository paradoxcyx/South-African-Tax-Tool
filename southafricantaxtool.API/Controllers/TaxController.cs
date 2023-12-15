using Microsoft.AspNetCore.Mvc;
using southafricantaxtool.API.Models;
using southafricantaxtool.API.Models.Tax.CalculateTax;
using southafricantaxtool.API.Models.Tax.CalculateTaxMetrics;
using southafricantaxtool.BL.Services.TaxLookup;
using southafricantaxtool.BL.TaxCalculation;
using southafricantaxtool.Interface;

namespace southafricantaxtool.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxController(ILogger<TaxController> logger, 
        ITaxCalculationService taxCalculationService, 
        ITaxLookupService taxLookupService, 
        ITaxBracketStore taxBracketStore, 
        ITaxRebateStore taxRebateStore) : ControllerBase
    {
        [HttpPost("CalculateTax")]
        public async Task<IActionResult> CalculateTax([FromBody] CalculateTaxInput input)
        {
            try
            {
                var taxBrackets = await taxBracketStore.GetAsync();
                var taxRebates = await taxRebateStore.GetAsync();
                
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

                var output = new CalculateTaxOutput
                {
                    AnnualTax = annualTax,
                    MonthlyTax = monthlyTax,
                    MonthlyNett = monthlyNett,
                    AnnualNett = annualNett,
                    Rule = rule,
                    FormulaSteps = taxCalculationService.BuildFormulaSteps(bracket, annualIncome, annualTax, rebate!.Amount)
                };

                var response = new GenericResponseModel<CalculateTaxOutput>
                {
                    Success = true,
                    Message = "Tax calculated successfully",
                    Data = output
                };

                return Ok(response);
            }
            catch (InvalidOperationException op)
            {
                return BadRequest(new GenericResponseModel<CalculateTaxOutput>
                {
                    Success = false,
                    Message = op.Message
                });
            }
        }

        [HttpPost("CalculateTaxMetrics")]
        public async Task<IActionResult> CalculateTaxMetrics([FromBody] CalculateTaxMetricsInput input)
        {
            try
            {
                var taxBrackets = await taxBracketStore.GetAsync();
                var taxRebates = await taxRebateStore.GetAsync();

                var monthlyIncome = input.IsMonthly ? input.Income : input.Income / 12;
                var annualIncome = input.IsMonthly ? input.Income * 12 : input.Income;

                var years = taxBrackets
                    .Where(x => x.End.HasValue)
                    .Select(x => x.End!.Value.Year)
                    .OrderByDescending(o => o)
                    .ToList();

                var metrics = new List<CalculateTaxMetricsOutput>();
                CalculateTaxMetricsOutput? previousMetric = null;

                foreach (var year in years)
                {
                    var taxBracket = taxLookupService.FindTaxBracketForYear(taxBrackets, year);
                    var bracket = taxLookupService.FindTaxBracketForIncome(taxBracket.Brackets, annualIncome);
                    var rebate = taxLookupService.FindTaxRebateForAgeAndYear(taxRebates, input.Age, year);

                    var annualTax = taxCalculationService.CalculateAnnualTax(bracket, annualIncome);
                    var monthlyTax = taxCalculationService.CalculateMonthlyTax(annualTax, rebate.Amount);
                    var monthlyNett = taxCalculationService.CalculateMonthlyNett(monthlyIncome, monthlyTax);
                    var annualNett = taxCalculationService.CalculateAnnualNett(annualIncome, annualTax, rebate.Amount);

                    var metric = new CalculateTaxMetricsOutput
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

                var response = new GenericResponseModel<List<CalculateTaxMetricsOutput>>
                {
                    Success = true,
                    Message = "Tax metrics calculated successfully",
                    Data = metrics
                };

                return Ok(response);
            }
            catch (InvalidOperationException op)
            {
                return BadRequest(new GenericResponseModel<List<CalculateTaxMetricsOutput>>
                {
                    Success = false,
                    Message = op.Message
                });
            }
            
        }
        
    }
}
