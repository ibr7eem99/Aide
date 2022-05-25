using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Aide.Extensions
{
    public  static class AdvicingMatelrialFolderMangment
    {
        public static string[] GetMajorsName(IWebHostEnvironment webHostEnvironment)
        {
            string advisingMaterialFolder = $@"{webHostEnvironment.WebRootPath}\AdvisingMaterial";
            if (!System.IO.Directory.Exists(advisingMaterialFolder))
            {
                System.IO.Directory.CreateDirectory(advisingMaterialFolder);
            }
            advisingMaterialFolder += $@"\Majors";
            if (!System.IO.Directory.Exists(advisingMaterialFolder))
            {
                System.IO.Directory.CreateDirectory(advisingMaterialFolder);
            }
            return System.IO.Directory.GetDirectories(advisingMaterialFolder);
        }

        public static void SavePlanFile(string planePath,IFormFile PlaneFile)
        {
            using (FileStream fs = new FileStream(planePath, FileMode.Create))
            {
                PlaneFile.CopyTo(fs);

            }
            

        }
    } 
}
