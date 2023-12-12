using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.BL.Services.TaxLookup;

public interface ITaxLookupService
{
    /// <summary>
    /// Finding tax bracket for specified year
    /// </summary>
    /// <param name="taxBrackets">Tax brackets source list</param>
    /// <param name="year">Specified year</param>
    /// <returns>The tax bracket</returns>
    /// <exception cref="InvalidOperationException">Error if no tax bracket exists for specified year</exception>
    public TaxBracket FindTaxBracketForYear(IEnumerable<TaxBracket> taxBrackets, int year);

    /// <summary>
    /// Finding tax bracket for specified annual income
    /// </summary>
    /// <param name="brackets">Tax brackets source list</param>
    /// <param name="income">Specified annual income</param>
    /// <returns>The tax bracket</returns>
    /// <exception cref="InvalidOperationException">Error if no tax bracket exists for specified annual income</exception>
    public Bracket FindTaxBracketForIncome(IEnumerable<Bracket> brackets, decimal income);

    /// <summary>
    /// Finding tax rebate for specified age and year
    /// </summary>
    /// <param name="taxRebates">Tax rebates source list</param>
    /// <param name="age">Specified age</param>
    /// <param name="year">Specified year</param>
    /// <returns>The tax rebate</returns>
    /// <exception cref="InvalidOperationException">Error if no tax rebate exists for specified age and/or year</exception>
    public Rebate FindTaxRebateForAgeAndYear(IEnumerable<TaxRebate> taxRebates, int age, int year);
}