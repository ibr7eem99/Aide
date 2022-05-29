using Aide.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Aide.Service.GraphAPIService
{
    public class GraphService
    {
        #region Get Items From OneDrive
        public static async Task<IEnumerable<DriveItem>> GetAllItemsInsideDrive(GraphServiceClient graphClient, IHttpContextAccessor httpContext)
        {
            try
            {
                IEnumerable<DriveItem> children = await graphClient.Me.Drive.Root.Children
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
                        await httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }

        public static async Task<IEnumerable<DriveItem>> GetItemInsideFolder(GraphServiceClient graphClient, IHttpContextAccessor httpContext, string itemId, string folderName)
        {
            try
            {
                string filterQuery = $@"name eq '{folderName}'";
                var children = await graphClient.Me.Drive.Items[itemId].Children
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
                        await httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }
        #endregion

        #region Create Folder inside OneDrive
        public static async Task<DriveItem> CreateFolderInsideDriveRoot(GraphServiceClient graphClient, IHttpContextAccessor httpContext, string folderName)
        {
            try
            {
                var driveItem = new DriveItem
                {
                    Name = folderName,
                    Folder = new Folder()
                };

                var responce = await graphClient.Me.Drive.Root.Children
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
                        await httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }

        public static async Task<DriveItem> CreatNewFolder(GraphServiceClient graphClient, IHttpContextAccessor httpContext, string itemId, string folderName)
        {
            try
            {
                var driveItem = new DriveItem
                {
                    Name = folderName,
                    Folder = new Folder()
                };

                var children = await graphClient.Me.Drive.Items[itemId].Children
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
                        await httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }
        #endregion

        public static async Task<DriveItem> UplaodAnExistingFile(GraphServiceClient graphClient, IHttpContextAccessor httpContext, string driveItemId, string filePath)
        {
            try
            {
                string path = filePath;
                using (var stream = new System.IO.FileStream(path, FileMode.Open))
                {
                    var item = await graphClient.Me.Drive.Items[$"{driveItemId}:/{20211}.xlsx:"].Content
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
                        await httpContext.HttpContext.ChallengeAsync();
                        throw new AuthenticationException(ex.Error);
                    default:
                        throw new Exception("An unknown error has occurred.");
                }
            }
        }

        // Load user's profile picture in base64 string.
        /*public static async Task<string> GetPictureBase64(GraphServiceClient graphClient, string email, HttpContext httpContext)
        {
            try
            {
                // Load user's profile picture.
                var pictureStream = await GetPictureStream(graphClient, email, httpContext);

                if (pictureStream == null) return PlaceholderImage;

                // Copy stream to MemoryStream object so that it can be converted to byte array.
                var pictureMemoryStream = new MemoryStream();
                await pictureStream.CopyToAsync(pictureMemoryStream);

                // Convert stream to byte array.
                var pictureByteArray = pictureMemoryStream.ToArray();

                // Convert byte array to base64 string.
                var pictureBase64 = Convert.ToBase64String(pictureByteArray);

                return "data:image/jpeg;base64," + pictureBase64;
            }
            catch (Exception e)
            {
                return e.Message switch
                {
                    "ResourceNotFound" => PlaceholderImage, // If picture is not found, return the placeholder image.
                    "EmailIsNull" => JsonConvert.SerializeObject(new { Message = "Email address cannot be null." }, Formatting.Indented),
                    _ => null,
                };
            }
        }*/

        /*public static async Task<Stream> GetPictureStream(GraphServiceClient graphClient, string email, HttpContext httpContext)
        {
            if (email == null) throw new Exception("EmailIsNull");

            Stream pictureStream = null;

            try
            {
                try
                {
                    // Load user's profile picture.
                    pictureStream = await graphClient.Users[email].Photo.Content.Request().GetAsync();
                }
                catch (ServiceException e)
                {
                    if (e.Error.Code == "GetUserPhoto") // User is using MSA, we need to use beta endpoint
                    {
                        // Set Microsoft Graph endpoint to beta, to be able to get profile picture for MSAs 
                        graphClient.BaseUrl = "https://graph.microsoft.com/beta";

                        // Get profile picture from Microsoft Graph
                        pictureStream = await graphClient.Users[email].Photo.Content.Request().GetAsync();

                        // Reset Microsoft Graph endpoint to v1.0
                        graphClient.BaseUrl = "https://graph.microsoft.com/v1.0";
                    }
                }
            }
            catch (ServiceException e)
            {
                switch (e.Error.Code)
                {
                    case "Request_ResourceNotFound":
                    case "ResourceNotFound":
                    case "ErrorItemNotFound":
                    case "itemNotFound":
                    case "ErrorInvalidUser":
                        // If picture not found, return the default image.
                        throw new Exception("ResourceNotFound");
                    case "TokenNotFound":
                        await httpContext.ChallengeAsync();
                        return null;
                    default:
                        return null;
                }
            }

            return pictureStream;
        }*/
        /*public static async Task<Stream> GetMyPictureStream(GraphServiceClient graphClient, HttpContext httpContext)
        {
            Stream pictureStream = null;

            try
            {
                try
                {
                    // Load user's profile picture.
                    pictureStream = await graphClient.Me.Photo.Content.Request().GetAsync();
                }
                catch (ServiceException e)
                {
                    if (e.Error.Code == "GetUserPhoto") // User is using MSA, we need to use beta endpoint
                    {
                        // Set Microsoft Graph endpoint to beta, to be able to get profile picture for MSAs 
                        graphClient.BaseUrl = "https://graph.microsoft.com/beta";

                        // Get profile picture from Microsoft Graph
                        pictureStream = await graphClient.Me.Photo.Content.Request().GetAsync();

                        // Reset Microsoft Graph endpoint to v1.0
                        graphClient.BaseUrl = "https://graph.microsoft.com/v1.0";
                    }
                }
            }
            catch (ServiceException e)
            {
                switch (e.Error.Code)
                {
                    case "Request_ResourceNotFound":
                    case "ResourceNotFound":
                    case "ErrorItemNotFound":
                    case "itemNotFound":
                    case "ErrorInvalidUser":
                        // If picture not found, return the default image.
                        throw new Exception("ResourceNotFound");
                    case "TokenNotFound":
                        await httpContext.ChallengeAsync();
                        return null;
                    default:
                        return null;
                }
            }
            return pictureStream;
        }*/
    }
}
