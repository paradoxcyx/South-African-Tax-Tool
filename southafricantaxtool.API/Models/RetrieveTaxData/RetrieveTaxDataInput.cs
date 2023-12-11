namespace southafricantaxtool.API.Models.RetrieveTaxData
{
    public class RetrieveTaxDataInput
    {
        public int Age { get; set; }
        public int TaxYear { get; set; }
        public decimal Income { get; set; }
        public bool IsMonthly { get; set; }
    }
}
