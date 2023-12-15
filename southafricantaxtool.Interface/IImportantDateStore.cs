using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace southafricantaxtool.Interface
{
    public interface IImportantDateStore
    {
        Task<List<ImportantDate>> GetAsync(Func<ImportantDate, bool>? filter = null);
        Task SetAsync(List<ImportantDate> importantDates);
    }
}
