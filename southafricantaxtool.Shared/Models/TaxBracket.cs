using southafricantaxtool.Shared;
using southafricantaxtool.Shared.Models;

namespace southafricantaxtool.Shared.Models
{
    public class TaxBracket
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public List<Bracket> Brackets { get; set; }
    }
}


