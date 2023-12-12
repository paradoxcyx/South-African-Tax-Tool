using southafricantaxtool.Scraper;
using southafricantaxtool.Scraper.Models;

namespace southafricantaxtool.Scraper.Models
{
    public class TaxBracket
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public List<Bracket> Brackets { get; set; }
    }
}


