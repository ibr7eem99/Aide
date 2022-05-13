using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace Aide.Service.OneDriveService
{
    public interface IOneDriveService
    {
        Task<object> GetProfessorFolder(HttpContext httpContext, GraphServiceClient graphServiceClient);
        Task<object> GetStudentFolder(HttpContext httpContext, GraphServiceClient graphServiceClient, string ProfessorfolderId, string studentfolderName);
    }
}
