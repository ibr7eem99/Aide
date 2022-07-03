using Aide.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Aide.Service.GraphAPIService
{
    public class GraphService : IGraphService
    {
        private readonly IGraphServiceClientFactory _graphServiceClientFactory;
        private readonly IHttpContextAccessor _httpContext;
        private GraphServiceClient graphServiceClient;

        public GraphService(IGraphServiceClientFactory graphServiceClientFactory, IHttpContextAccessor httpContext)
        {
            _graphServiceClientFactory = graphServiceClientFactory;
            _httpContext = httpContext;
            graphServiceClient = _graphServiceClientFactory.GetAuthenticatedGraphClient((ClaimsIdentity)_httpContext.HttpContext.User.Identity);
        }

        #region Get Items From OneDrive
        /*
         * Summary:
         *      Get the GraphServiceUsers request, response should contains 
         *      Shared folder info from root drive using microsoft Graph API
         */
        public async Task<IEnumerable<DriveItem>> GetFolderFromRootDrive()
        {
            try
            {
                IEnumerable<DriveItem> children = await graphServiceClient
                                                        .Me.Drive.Root.Children
                                                        .Request()
                                                        .Filter("name eq 'Shared'")
                                                        .GetAsync();
                return children;
            }
            catch (ServiceException ex)
            {
                switch (ex.Error.Code)
                {
                    case "AuthenticationFailure":
                        throw new AuthenticationException(ex.Error);
                    case "TokenNotFound":
                        // Genarate Authentication Exception if a microsoft access token not found in the cookie.
                        await _httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }

        /* 
         * Summary:
         *      Get the GraphServiceUsers request,
         *      response should contains an information for the folder in side the Onedrive
         *      using microsoft Graph API.
         * Parameters:
         *      itemId: specific Id for the parant folder
         *      folderName: folder name that I want to get it's information
         */
        public async Task<IEnumerable<DriveItem>> GetItemInsideFolder(string itemId, string folderName)
        {
            try
            {
                string filterQuery = $@"name eq '{folderName}'";
                var children = await graphServiceClient
                                    .Me.Drive.Items[itemId].Children
                                    .Request()
                                    .Filter(filterQuery)
                                    .GetAsync();

                return children;
            }
            catch (ServiceException ex)
            {
                switch (ex.Error.Code)
                {
                    case "AuthenticationFailure":
                        throw new AuthenticationException(ex.Error);
                    case "TokenNotFound":
                        // Genarate Authentication Exception if a microsoft access token not found in the cookie.
                        await _httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    case "invalidRequest":
                        throw new ServiceException(new Error { Message = "", Code = ex.Error.Code }); // TODO
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }
        #endregion

        #region Create Folder inside OneDrive
        /*
         * Summary:
         *      Microsoft Graph API request to create new folder inside root folder in onedrive.
         *  Parameters:
         *      folderName: is a name for the new folder that shoud create.
         */
        public async Task<DriveItem> CreateFolderInsideDriveRoot(string folderName)
        {
            try
            {
                var driveItem = new DriveItem
                {
                    Name = folderName,
                    Folder = new Folder()
                };

                var responce = await graphServiceClient
                                    .Me.Drive.Root.Children
                                    .Request()
                                    .AddAsync(driveItem);

                return responce;
            }
            catch (ServiceException ex)
            {
                switch (ex.Error.Code)
                {
                    case "AuthenticationFailure":
                        throw new AuthenticationException(ex.Error);
                    case "TokenNotFound":
                        await _httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }

        /*
         * Summary:
         *      Microsoft Graph API request to Create new folder inside specific folder.
         * Parameters:
         *      itemId: a specific Id for the parant folder who I want to create folder inside it
         *      folderName: Folder name for new folder.
         */
        public async Task<DriveItem> CreatNewFolder(string itemId, string folderName)
        {
            try
            {
                var driveItem = new DriveItem
                {
                    Name = folderName,
                    Folder = new Folder()
                };

                var children = await graphServiceClient
                                    .Me.Drive.Items[itemId].Children
                                    .Request()
                                    .AddAsync(driveItem);

                return children;
            }
            catch (ServiceException ex)
            {
                switch (ex.Error.Code)
                {
                    case "AuthenticationFailure":
                        throw new AuthenticationException(ex.Error);
                    case "TokenNotFound":
                        await _httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }
        #endregion

        /*
         * Summary:
         *      Microsoft Graph API request to uplade file inside specific folder in Onedrive.
         * Parameters:
         *      driveItemId: specific Id for the parant folder who I want to upload folder inside it
         *      filePath: is a path for the file that sholud upload it.
         */
        public async Task<DriveItem> UplaodAnExistingFile(string driveItemId, string filePath)
        {
            try
            {
                string path = filePath;
                using (var stream = new System.IO.FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    var item = await graphServiceClient
                                    .Me.Drive.Items[$"{driveItemId}:/{20211}.xlsx:"].Content
                                    .Request()
                                    .PutAsync<DriveItem>(stream);

                    return item;
                }
            }
            catch (ServiceException ex)
            {
                switch (ex.Error.Code)
                {
                    case "AuthenticationFailure":
                        throw new AuthenticationException(ex.Error);
                    case "TokenNotFound":
                        await _httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }

        /*
         * Summary:
         *      Microsoft Graph API request to delete file inside specific folder in Onedrive.
         * Parameters:
         *      driveItemId: specific Id for the parant folder who I want to delete folder inside it.
         */
        public async Task DeleteAnExistingFile(string driveItemId)
        {
            await graphServiceClient.Me.Drive.Items[$"{driveItemId}"]
                    .Request()
                    .DeleteAsync();
        }
    }
}
