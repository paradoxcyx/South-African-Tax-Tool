using southafricantaxtool.Interface.Models;

namespace southafricantaxtool.Interface.Services
{
    public interface ITaxBracketStore
    {
        Task<List<TaxBracket>> GetAsync();
        Task SetAsync(List<TaxBracket> taxBrackets);
    }
}
