using Aide.Data;
using Aide.Extensions;
using Aide.Service.GraphAPIService;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
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
        public async Task<object> GetProfessorFolder(GraphServiceClient graphServiceClient, string professorName)
        {
            DriveItem drive = null;
            if (professorName is not null)
            {
                var graphClient = graphServiceClient;
                DriveItem sharedFolder = await GetFolderFromRootDrive(graphClient);
                string jsonString = await GraphService.GetAllItemsInsideFolder(graphClient, _httpContextAccessor, sharedFolder.Id);
                try
                {
                    IEnumerable<DriveItem> foldersInfo = null;
                    foldersInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<DriveItem>>(jsonString);
                    foreach (DriveItem item in foldersInfo)
                    {
                        if (item.Name == professorName)
                        {
                            drive = item;
                        }
                    }

                    if (drive is null)
                    {
                        jsonString = await GraphService.CreatNewFolder(graphClient, sharedFolder.Id, professorName);
                        drive = Newtonsoft.Json.JsonConvert.DeserializeObject<DriveItem>(jsonString);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionMessage message = null;
                    message = Newtonsoft.Json.JsonConvert.DeserializeObject<ExceptionMessage>(jsonString);
                    return message;
                }
            }

            return drive;
        }

        private async Task<DriveItem> GetFolderFromRootDrive(GraphServiceClient graphServiceClient)
        {
            DriveItem drive = null;
            /*if (httpContext is not null)
            {*/
            var graphClient = graphServiceClient;
            string jsonString = await GraphService.GetAllItemsInsideDrive(graphClient, _httpContextAccessor);
            IEnumerable<DriveItem> foldersInfo = null;
            foldersInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<DriveItem>>(jsonString);
            foreach (DriveItem item in foldersInfo)
            {
                if (item.Name == "Shared")
                {
                    drive = item;
                }
            }

            if (drive is null)
            {
                jsonString = await GraphService.CreateFolderInsideDriveRoot(graphClient, _httpContextAccessor, "Shared");
                drive = Newtonsoft.Json.JsonConvert.DeserializeObject<DriveItem>(jsonString);
            }
            /*}*/
            return drive;
        }
        #endregion

        #region StudentsFolder
        public async Task<object> GetStudentFolder(GraphServiceClient graphServiceClient, string ProfessorfolderId, string studentfolderName)
        {
            DriveItem drive = null;

            var graphClient = graphServiceClient;
            string jsonString = await GraphService.GetAllItemsInsideFolder(graphClient, _httpContextAccessor, ProfessorfolderId);
            try
            {
                IEnumerable<DriveItem> foldersInfo = null;
                foldersInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<DriveItem>>(jsonString);
                foreach (DriveItem item in foldersInfo)
                {
                    if (item.Name.Equals(studentfolderName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        drive = item;
                    }
                }

                if (drive is null)
                {
                    jsonString = await GraphService.CreatNewFolder(graphClient, ProfessorfolderId, studentfolderName);
                    drive = Newtonsoft.Json.JsonConvert.DeserializeObject<DriveItem>(jsonString);
                }
            }
            catch (Exception ex)
            {
                ExceptionMessage message = null;
                message = Newtonsoft.Json.JsonConvert.DeserializeObject<ExceptionMessage>(jsonString);
                return message;
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
