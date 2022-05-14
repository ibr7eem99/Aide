using Aide.Data;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aide.Service.ExcelSheetService
{
    public interface IStudyPlan
    {
        public Task<bool> GenarateExcelSheet(IEnumerable<Supuervised> supuerviseds, string professorName, GraphServiceClient graphServiceClient);
    }
}
