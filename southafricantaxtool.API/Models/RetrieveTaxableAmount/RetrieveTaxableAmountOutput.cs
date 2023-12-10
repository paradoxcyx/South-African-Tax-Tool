namespace southafricantaxtool.API.Models.RetrieveTaxableAmount
{
    public class RetrieveTaxableAmountOutput
    {
        public decimal AnnualTax { get; set; }
        public decimal MonthlyTax { get; set; }
        public decimal MonthlyNett { get; set; }
        public decimal AnnualNett { get; set; }
        public List<string> FormulaSteps { get; set; }
    }
}
