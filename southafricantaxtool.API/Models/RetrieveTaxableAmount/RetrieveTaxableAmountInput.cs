namespace southafricantaxtool.API.Models.RetrieveTaxableAmount
{
    public class RetrieveTaxableAmountInput
    {
        public int Year { get; set; }
        public decimal Income { get; set; }
        public bool IsMonthly { get; set; }
    }
}
