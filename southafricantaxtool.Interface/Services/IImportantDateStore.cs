using southafricantaxtool.Interface.Models;

namespace southafricantaxtool.Interface.Services
{
    public interface IImportantDateStore
    {
        Task<List<ImportantDate>> GetAsync(Func<ImportantDate, bool>? filter = null);
        Task SetAsync(List<ImportantDate> importantDates);
    }
}
