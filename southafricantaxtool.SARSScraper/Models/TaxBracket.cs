using southafricantaxtool.SARSScraper;
using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.SARSScraper.Models
{
    public class TaxBracket
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public List<Bracket> Brackets { get; set; }
    }
}


