using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using southafricantaxtool.SARSScraper.Models;

namespace southafricantaxtool.Interface
{
    public interface ITaxBracketStore
    {
        Task<List<TaxBracket>> GetAsync();
        Task SetAsync(List<TaxBracket> taxBrackets);
    }
}
