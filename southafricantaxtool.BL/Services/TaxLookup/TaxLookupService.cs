using southafricantaxtool.Scraper.Enums;
using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.BL.Services.TaxLookup;

public class TaxLookupService : ITaxLookupService
{
    public TaxBracket FindTaxBracketForYear(IEnumerable<TaxBracket> taxBrackets, int year)
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

    public Bracket FindTaxBracketForIncome(IEnumerable<Bracket> brackets, decimal income)
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

    public Rebate FindTaxRebateForAgeAndYear(IEnumerable<TaxRebate> taxRebates, int age, int year)
    {
        var taxRebate = age switch
        {
            >= 0 and <= 65 => taxRebates.FirstOrDefault(x => x.TaxRebateType == TaxRebateEnum.Primary),
            > 65 and <= 75 => taxRebates.FirstOrDefault(
                x => x.TaxRebateType == TaxRebateEnum.Secondary),
            > 75 => taxRebates.FirstOrDefault(x => x.TaxRebateType == TaxRebateEnum.Tertiary),
            _ => throw new InvalidOperationException("Unable to find tax rebate for specified age")
        };

        if (taxRebate == null) 
            throw new InvalidOperationException("Unable to find tax rebate for specified age");

        var taxRebateForYear = taxRebate.Rebates.FirstOrDefault(x => x.Year == year);

        if (taxRebateForYear == null)
            throw new InvalidOperationException("Unable to find tax rebate for specified year");
        
        return taxRebateForYear;
    }
}