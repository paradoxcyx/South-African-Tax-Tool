using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace southafricantaxtool.Interface
{
    internal interface ITaxBracketStore
    {
        Task<List<TaxBracket>> GetAsync();
        Task SetAsync(List<TaxBracket> taxBrackets);
        Task UpdateRedisCache(List<TaxBracket> taxBrackets);
    }
}
