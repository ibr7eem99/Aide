using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace Aide.Service.OneDriveService
{
    public interface IOneDriveService
    {
        Task<DriveItem> GetProfessorFolder(string professorName);
        Task<DriveItem> GetStudentFolder(string ProfessorfolderId, string studentfolderName);
        Task UplodExcelSheet(string studentFolderId, string ecxelSheetPath);
    }


}
