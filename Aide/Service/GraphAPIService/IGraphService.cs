using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aide.Service.GraphAPIService
{
    public interface IGraphService
    {
        public Task<IEnumerable<DriveItem>> GetAllItemsInsideDrive();
        public Task<IEnumerable<DriveItem>> GetItemInsideFolder(string itemId, string folderName);
        public Task<DriveItem> CreateFolderInsideDriveRoot(string folderName);
        public Task<DriveItem> CreatNewFolder(string itemId, string folderName);
        public Task<DriveItem> UplaodAnExistingFile(string driveItemId, string filePath);
    }
}
