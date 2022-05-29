using Aide.Controllers;
using Aide.Data;
using Aide.Extensions;
using Aide.Service.GraphAPIService;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aide.Service.OneDriveService
{
    public class OneDriveService : IOneDriveService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OneDriveService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #region ProfessorFolder
        private async Task<DriveItem> GetFolderFromRootDrive(GraphServiceClient graphServiceClient)
        {
            DriveItem drive = null;
            IEnumerable<DriveItem> foldersInfo = null;
            var graphClient = graphServiceClient;

            foldersInfo = await GraphService.GetAllItemsInsideDrive(graphClient, _httpContextAccessor);
            if (Enumerable.Any(foldersInfo))
            {
                drive = Enumerable.FirstOrDefault(foldersInfo);
            }
            else
            {
                drive = await GraphService.CreateFolderInsideDriveRoot(graphClient, _httpContextAccessor, "Shared");
            }
            return drive;
        }

        public async Task<DriveItem> GetProfessorFolder(GraphServiceClient graphServiceClient, string professorName)
        {
            DriveItem drive = null;
            if (professorName is not null)
            {
                var graphClient = graphServiceClient;
                DriveItem sharedFolder = await GetFolderFromRootDrive(graphClient);
                IEnumerable<DriveItem> foldersInfo = await GraphService.GetItemInsideFolder(graphClient, _httpContextAccessor, sharedFolder.Id, professorName);
                if (Enumerable.Any(foldersInfo))
                {
                    drive = Enumerable.FirstOrDefault(foldersInfo);
                }
                else
                {
                    drive = await GraphService.CreatNewFolder(graphClient, _httpContextAccessor, sharedFolder.Id, professorName);
                }
            }
            return drive;
        }
        #endregion

        #region StudentsFolder
        public async Task<DriveItem> GetStudentFolder(GraphServiceClient graphServiceClient, string ProfessorfolderId, string studentfolderName)
        {
            DriveItem drive = null;
            var graphClient = graphServiceClient;
            IEnumerable<DriveItem> foldersInfo = await GraphService.GetItemInsideFolder(graphClient, _httpContextAccessor, ProfessorfolderId, studentfolderName);

            if (Enumerable.Any(foldersInfo))
            {
                drive = Enumerable.FirstOrDefault(foldersInfo);
            }
            else
            {
                drive = await GraphService.CreatNewFolder(graphClient, _httpContextAccessor, ProfessorfolderId, studentfolderName);
            }

            return drive;
        }
        #endregion

        public async Task<bool> UplodExcelSheet(GraphServiceClient graphServiceClient, string studentFolderId, string ecxelSheetPath)
        {
            var graphClient = graphServiceClient;
            await GraphService.UplaodAnExistingFile(graphClient, _httpContextAccessor, studentFolderId, ecxelSheetPath);

            return true;
        }
    }
}
