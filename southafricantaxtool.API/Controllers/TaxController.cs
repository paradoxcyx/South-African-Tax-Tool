using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using southafricantaxtool.API.Models;
using southafricantaxtool.API.Models.RetrieveTaxData;
using southafricantaxtool.Shared;
using southafricantaxtool.Shared.Models;

namespace southafricantaxtool.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaxController(ILogger<TaxController> logger, IDistributedCache cache) : ControllerBase
    {
        private readonly ILogger<TaxController> _logger = logger;
        private readonly IDistributedCache _cache = cache;
        
        /// <summary>
        /// Finding tax bracket for specified year
        /// </summary>
        /// <param name="taxBrackets">Tax brackets source list</param>
        /// <param name="year">Specified year</param>
        /// <returns>The tax bracket</returns>
        /// <exception cref="InvalidOperationException">Error if no tax bracket exists for specified year</exception>
        private TaxBracket FindTaxBracketForYear(IEnumerable<TaxBracket> taxBrackets, int year)
        {
            var taxBracket = taxBrackets.FirstOrDefault(x =>
                x.Start?.Year is { } startYear &&
                x.End?.Year is { } endYear &&
                startYear <= year && endYear >= year);

            if (taxBracket == null)
            {
                throw new InvalidOperationException($"Unable to find tax bracket for year: {year}");
            }

            return taxBracket;
        }

        /// <summary>
        /// Finding tax bracket for specified annual income
        /// </summary>
        /// <param name="brackets">Tax brackets source list</param>
        /// <param name="income">Specified annual income</param>
        /// <returns>The tax bracket</returns>
        /// <exception cref="InvalidOperationException">Error if no tax bracket exists for specified annual income</exception>
        private Bracket FindTaxBracketForIncome(IEnumerable<Bracket> brackets, decimal income)
        {
            var bracket = brackets.FirstOrDefault(x =>
                (x.IncomeFrom <= income && x.IncomeTo >= income) ||
                (x.IncomeFrom <= income && !x.IncomeTo.HasValue));

            if (bracket == null)
            {
                throw new InvalidOperationException("Unable to find tax bracket for specified income");
            }

            return bracket;
        }

        /// <summary>
        /// Finding tax rebate for specified age and year
        /// </summary>
        /// <param name="taxRebates">Tax rebates source list</param>
        /// <param name="age">Specified age</param>
        /// <param name="year">Specified year</param>
        /// <returns>The tax rebate</returns>
        /// <exception cref="InvalidOperationException">Error if no tax rebate exists for specified age and/or year</exception>
        private Rebate FindTaxRebateForAgeAndYear(IEnumerable<TaxRebate> taxRebates, int age, int year)
        {
            var taxRebate = age switch
            {
                >= 0 and <= 65 => taxRebates.FirstOrDefault(x => x.TaxRebateType == Shared.Enums.TaxRebateEnum.Primary),
                > 65 and <= 75 => taxRebates.FirstOrDefault(
                    x => x.TaxRebateType == Shared.Enums.TaxRebateEnum.Secondary),
                > 75 => taxRebates.FirstOrDefault(x => x.TaxRebateType == Shared.Enums.TaxRebateEnum.Tertiary),
                _ => throw new InvalidOperationException("Unable to find tax rebate for specified age")
            };

            if (taxRebate == null) 
                throw new InvalidOperationException("Unable to find tax rebate for specified age");

            var taxRebateForYear = taxRebate.Rebates.FirstOrDefault(x => x.Year == year);

            if (taxRebateForYear == null)
                throw new InvalidOperationException("Unable to find tax rebate for specified year");
            
            return taxRebateForYear;
        }

        private async Task<TaxData> GetTaxData()
        {
            TaxData? taxData;

            var s = await _cache.GetAsync("taxdata");
            if (s == null)
            {
                taxData = await TaxScraper.RetrieveTaxData();
                await _cache.SetAsync("taxdata", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(taxData)), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                });
            }
            else
            {
                var json = Encoding.UTF8.GetString(s);
                taxData = JsonConvert.DeserializeObject<TaxData>(json);
            }

            if (taxData == null)
                throw new InvalidOperationException("Unable to retrieve tax data");
            
            return taxData;
        }

        [HttpPost("RetrieveTaxData")]
        public async Task<IActionResult> RetrieveTaxData([FromBody] RetrieveTaxDataInput input)
        {
            try
            {
                var taxData = await GetTaxData();
                
                var monthlyIncome = input.IsMonthly ? input.Income : input.Income / 12;
                var annualIncome = input.IsMonthly ? input.Income * 12 : input.Income;

                var taxBracket = FindTaxBracketForYear(taxData.TaxBrackets, input.TaxYear);
                var bracket = FindTaxBracketForIncome(taxBracket.Brackets, annualIncome);
                var rebate = FindTaxRebateForAgeAndYear(taxData.TaxRebates, input.Age, input.TaxYear);


                var annualTax = bracket.Rule.BaseAmount.HasValue
                    ? bracket.Rule.BaseAmount.Value +
                      (annualIncome - bracket.IncomeFrom) * bracket.Rule.Percentage / 100
                    : Math.Round(annualIncome * ((decimal)bracket.Rule.Percentage / 100), 2);

                var monthlyTax = Math.Round((annualTax - rebate.Amount) / 12, 2);
                
                var monthlyNett =
                    Math.Round(monthlyIncome - monthlyTax, 2);

                var annualNett = annualIncome - (annualTax - rebate.Amount);

                var rule = bracket.Rule.BaseAmount.HasValue
                    ? $"{bracket.Rule.BaseAmount.Value:F2} + {bracket.Rule.Percentage}% of taxable income above {bracket.IncomeFrom:F2}"
                    : $"{bracket.Rule.Percentage}% of Taxable Income";

                var output = new RetrieveTaxDataOutput
                {
                    AnnualTax = annualTax,
                    MonthlyTax = monthlyTax,
                    MonthlyNett = monthlyNett,
                    AnnualNett = annualNett,
                    Rule = rule,
                    FormulaSteps = BuildFormulaSteps(bracket, annualIncome, annualTax, rebate!.Amount)
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
                _logger.LogError(op.Message);

                return BadRequest(new GenericResponseModel<RetrieveTaxDataOutput>
                {
                    Success = false,
                    Message = op.Message
                });
            }
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
