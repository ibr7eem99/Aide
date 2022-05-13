using Aide.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aide.Service.ExcelSheetService
{
    public interface IStudyPlan
    {
        public Task<bool> GenarateExcelSheet(IEnumerable<Supuervised> supuerviseds);
    }
}
