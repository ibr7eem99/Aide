﻿using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace Aide.Service.OneDriveService
{
    public interface IOneDriveService
    {
        Task<object> GetProfessorFolder(GraphServiceClient graphServiceClient, string professorName);
        Task<object> GetStudentFolder(GraphServiceClient graphServiceClient, string ProfessorfolderId, string studentfolderName);

        Task<bool> UplodExcelSheet(GraphServiceClient graphServiceClient, string studentFolderId, string ecxelSheetPath);
        
    

    }


}
