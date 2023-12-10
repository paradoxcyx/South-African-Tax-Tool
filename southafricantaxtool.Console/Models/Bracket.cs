using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SARSTaxBracketScraper.Models
{
    public class Bracket
    {
        public decimal IncomeFrom { get; set; }
        public decimal? IncomeTo { get; set; }
        public BracketRule Rule { get; set; }
    }
}
