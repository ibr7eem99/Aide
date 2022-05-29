using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace Aide.Service.OneDriveService
{
    public interface IOneDriveService
    {
        Task<DriveItem> GetProfessorFolder(GraphServiceClient graphServiceClient, string professorName);
        Task<DriveItem> GetStudentFolder(GraphServiceClient graphServiceClient, string ProfessorfolderId, string studentfolderName);

        Task<bool> UplodExcelSheet(GraphServiceClient graphServiceClient, string studentFolderId, string ecxelSheetPath);
    }


}
