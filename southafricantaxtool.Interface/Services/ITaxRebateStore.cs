using southafricantaxtool.Interface.Models;

namespace southafricantaxtool.Interface.Services
{
    public interface ITaxRebateStore
    {
        Task<List<TaxRebate>> GetAsync();
        Task SetAsync(List<TaxRebate> taxRebates);
    }
}
