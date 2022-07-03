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
        /*
         * Summary:
         *      Check if root folder is contains shared folder,
         *      if not it call function to create new folder inside root folder,
         *      then return DriveItem object that contains folder info.
         */
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
        /*
         * Summary:
         *      Check if shared folder is contains folder name equal to professor name,
         *      the value of professor name should get from the cookies,
         *      if not it call function to create new folder for the professor,
         *      then return DriveItem object that contains folder info.
         *  Parameters:
         *          professorName: a professor name that sholud search for,
         *          or folder name for new folder that will create if the professor folder is not exist.
         */
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
        /*
         * Summary:
         *      Check if professor folder is contains folder name equal to student name,
         *      if not it should call function to create new folder for the student,
         *      then return DriveItem object that contains folder info.
         * Parameters:
         *      ProfessorfolderId: a specific Id for the professor folder.
         *      studentfolderName: a student name that sholud search for,
         *          or folder name for new folder that will create if the student folder is not exist.
         */
        public async Task<DriveItem> GetStudentFolder(string ProfessorfolderId, string studentfolderName)
        {
            if (string.IsNullOrEmpty(ProfessorfolderId))
            {
                throw new ArgumentNullException(nameof(ProfessorfolderId));
            }

            if (string.IsNullOrEmpty(studentfolderName))
            {
                throw new ArgumentNullException(nameof(studentfolderName), "Student Folder Name should not be empty");
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
