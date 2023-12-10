using southafricantaxtool.CMD;
using southafricantaxtool.CMD.Models;

namespace southafricantaxtool.CMD.Models
{
    public class TaxBracket
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public List<Bracket> Brackets { get; set; }
    }
}


