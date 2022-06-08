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
        /*private readonly IHttpContextAccessor _httpContextAccessor;*/
        private readonly IGraphService _graphService;

        public OneDriveService(IGraphService graphService)
        {
            /*_httpContextAccessor = httpContextAccessor;*/
            _graphService = graphService;
            /*Console.WriteLine("This is a OneDriveService Class");*/
        }

        #region ProfessorFolder
        private async Task<DriveItem> GetFolderFromRootDrive()
        {
            DriveItem drive = null;
            IEnumerable<DriveItem> foldersInfo = null;

            foldersInfo = await _graphService.GetAllItemsInsideDrive();
            if (Enumerable.Any(foldersInfo))
            {
                drive = Enumerable.FirstOrDefault(foldersInfo);
            }
            else
            {
                drive = await _graphService.CreateFolderInsideDriveRoot("Shared");
            }
            return drive;
        }

        public async Task<DriveItem> GetProfessorFolder(string professorName)
        {
            DriveItem drive = null;
            DriveItem sharedFolder = await GetFolderFromRootDrive();
            IEnumerable<DriveItem> foldersInfo = await _graphService.GetItemInsideFolder(sharedFolder.Id, professorName);
            if (Enumerable.Any(foldersInfo))
            {
                drive = Enumerable.FirstOrDefault(foldersInfo);
            }
            else
            {
                drive = await _graphService.CreatNewFolder(sharedFolder.Id, professorName);
            }

            return drive;
        }
        #endregion

        #region StudentsFolder
        public async Task<DriveItem> GetStudentFolder(string ProfessorfolderId, string studentfolderName)
        {
            if (string.IsNullOrEmpty(ProfessorfolderId))
            {
                /*throw new ArgumentNullException("Professor Id should not be empty");*/
                throw new ArgumentNullException(nameof(ProfessorfolderId));
            }

            if (string.IsNullOrEmpty(studentfolderName))
            {
                throw new ArgumentNullException("Student Folder Name should not be empty");
            }

            DriveItem drive = null;
            IEnumerable<DriveItem> foldersInfo = await _graphService.GetItemInsideFolder(ProfessorfolderId, studentfolderName);

            if (Enumerable.Any(foldersInfo))
            {
                drive = Enumerable.FirstOrDefault(foldersInfo);
            }
            else
            {
                drive = await _graphService.CreatNewFolder(ProfessorfolderId, studentfolderName);
            }

            return drive;
        }
        #endregion

        public async Task UplodExcelSheet(string studentFolderId, string ecxelSheetPath)
        {
            if (string.IsNullOrEmpty(studentFolderId))
            {
                /*throw new ArgumentNullException("Professor Id should not be empty");*/
                throw new ArgumentNullException(nameof(studentFolderId));
            }

            if (string.IsNullOrEmpty(ecxelSheetPath))
            {
                throw new ArgumentNullException("Ecxel Sheet Path should not be empty");
            }

            await _graphService.UplaodAnExistingFile(studentFolderId, ecxelSheetPath);
        }
    }
}
