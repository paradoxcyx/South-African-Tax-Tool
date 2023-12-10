using southafricantaxtool.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace southafricantaxtool.Shared.Models
{
    public class TaxRebate
    {
        public TaxRebateEnum TaxRebateType { get; set; }

        public List<Rebate> Rebates { get; set; }
    }
}
