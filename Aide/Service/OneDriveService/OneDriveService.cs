using Aide.Controllers;
using Aide.Data;
using Aide.Extensions;
using Aide.Service.GraphAPIService;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aide.Service.OneDriveService
{
    public class OneDriveService : IOneDriveService
    {
        private readonly IGraphService _graphService;

        public OneDriveService(IGraphService graphService)
        {
            _graphService = graphService;
        }

        #region ProfessorFolder
        private async Task<DriveItem> GetFolderFromRootDrive()
        {
            DriveItem drive = null;
            IEnumerable<DriveItem> foldersInfo = null;

            foldersInfo = await _graphService.GetFolderFromRootDrive();
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
                throw new ArgumentNullException(nameof(ProfessorfolderId));
            }

            if (string.IsNullOrEmpty(studentfolderName))
            {
                throw new ArgumentNullException("Student Folder Name should not be empty");
            }

            DriveItem drive = null;
            DriveItem foldersInfo = await GetExistingFile(ProfessorfolderId, studentfolderName);

            if (foldersInfo is not null)
            {
                drive = foldersInfo;
            }
            else
            {
                drive = await _graphService.CreatNewFolder(ProfessorfolderId, studentfolderName);
            }

            return drive;
        }
        #endregion

        public async Task UplodExcelSheet(string studentFolderId, string excelSheetPath)
        {
            if (string.IsNullOrEmpty(studentFolderId))
            {
                throw new ArgumentNullException(nameof(studentFolderId));
            }

            if (string.IsNullOrEmpty(excelSheetPath))
            {
                throw new ArgumentNullException(nameof(excelSheetPath), "Ecxel Sheet Path should not be empty");
            }

            DriveItem excelSheet = await GetExistingFile(studentFolderId, Path.GetFileName(excelSheetPath));
            if (excelSheet is not null)
            {
                await _graphService.DeleteAnExistingFile(excelSheet.Id);
            }
            await _graphService.UplaodAnExistingFile(studentFolderId, excelSheetPath);
        }

        private async Task<DriveItem> GetExistingFile(string studentFolderId, string fileName)
        {
            IEnumerable<DriveItem> file = await _graphService.GetItemInsideFolder(studentFolderId, fileName);
            if (file.Any())
            {
                return file.FirstOrDefault();
            }

            return null;
        }
    }
}
