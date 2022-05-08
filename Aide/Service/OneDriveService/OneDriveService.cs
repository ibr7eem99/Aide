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
        /*private readonly IGraphServiceClientFactory _graphServiceClientFactory;*/
        private HttpContext _httpContext = null;

        /*public OneDriveService(IGraphServiceClientFactory graphServiceClientFactory)
        {
            _graphServiceClientFactory = graphServiceClientFactory;
        }*/

        public async Task<object> GetProfessorFolder(HttpContext httpContext, GraphServiceClient graphServiceClient)
        {
            _httpContext = httpContext;
            DriveItem drive = null;

            if (_httpContext is not null)
            {
                string professorName = GetProfissorName();
                if (professorName is not null)
                {
                    var graphClient = graphServiceClient;
                    DriveItem sharedFolder = await GetFolderFromRootDrive(httpContext, graphClient);
                    string jsonString = await GraphService.GetAllItemsInsideFolder(graphClient, httpContext, sharedFolder.Id);
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
                            jsonString = await GraphService.CreatNewFolder(graphClient, httpContext, sharedFolder.Id, professorName);
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
            }
            return drive;
        }

        private string GetProfissorName()
        {
            byte[] tokenbyts = null;
            string tokenvalue = null;
            if (_httpContext.Session.TryGetValue("user", out tokenbyts))
            {
                tokenvalue = System.Text.Encoding.ASCII.GetString(tokenbyts);
                return tokenvalue;
            }
            return null;
        }

        private async Task<DriveItem> GetFolderFromRootDrive(HttpContext httpContext, GraphServiceClient graphServiceClient)
        {
            DriveItem drive = null;
            /*if (httpContext is not null)
            {*/
            var graphClient = graphServiceClient;
            string jsonString = await GraphService.GetAllItemsInsideDrive(graphClient, httpContext);
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
                jsonString = await GraphService.CreateFolderInsideDriveRoot(graphClient, httpContext, "Shared");
                drive = Newtonsoft.Json.JsonConvert.DeserializeObject<DriveItem>(jsonString);
            }
            /*}*/
            return drive;
        }
    }

    public interface IOneDriveService
    {
        Task<object> GetProfessorFolder(HttpContext httpContext, GraphServiceClient graphServiceClient);
    }
}
