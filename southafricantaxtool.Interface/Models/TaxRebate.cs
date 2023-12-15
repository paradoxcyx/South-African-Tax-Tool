using southafricantaxtool.Interface.Enums;
namespace southafricantaxtool.Interface.Models
{
    public class TaxRebate
    {
        public TaxRebateEnum TaxRebateType { get; set; }

        public List<Rebate> Rebates { get; set; }
    }
}
