using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Aide.Extensions
{
    public static class AdvicingMatelrialFolderMangment
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

        public static void SavePlanFile(string planePath, IFormFile PlaneFile)
        {
            using (FileStream fs = new FileStream(planePath, FileMode.Create))
            {
                PlaneFile.CopyTo(fs);
            }
        }

        public static void CreateNewDirectory(string planepath)
        {
            System.IO.Directory.CreateDirectory(planepath);
        }

        public static string CombineNewFileNameToPlanePath(string planType, string planepath, int planSemeter)
        {
            switch (planType)
            {
                case "TreePlan":
                    planepath = Path.Combine(planepath, $@"Flow_chart_Plan_{planSemeter}-{planSemeter + 1}.pdf");
                    break;
                case "StudyPlan":
                    planepath = Path.Combine(planepath, $@"{planSemeter}-{planSemeter + 1}.xlsx");
                    break;
            }
            return planepath;
        }
    }
}
