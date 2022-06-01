using Aide.Data;
using Aide.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Aide.Controllers
{
    public class PlansController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PlansController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [Route("[controller]/[action]/{planType}")]
        public IActionResult UplodFile(string planType)
        {
            if (!string.IsNullOrEmpty(planType))
            {
                if (planType.Equals("StudyPlan") || planType.Equals("TreePlan"))
                {
                    ViewData["PlanType"] = planType;
                    ViewData["MajorsName"] = AdvicingMatelrialFolderMangment.GetMajorsName(_webHostEnvironment);
                    return View();
                }
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [Route("[controller]/[action]/{planType}")]
        public IActionResult UplodFile(string planType, [Bind("MajorName,NewMajorName,PlanSemester,PlanFile,IsActive")] UplodPlanFile uplodPlanFile)
        {
            if (!string.IsNullOrEmpty(planType))
            {
                // Get Plan Type (TreePlane or StudyPlane) to use it in _Layout shared view
                ViewData["PlanType"] = planType;
                // Get Majors Name from wwwroot folder and use it in _Layout shared view to show the tree plan for each major
                ViewData["MajorsName"] = AdvicingMatelrialFolderMangment.GetMajorsName(_webHostEnvironment);

                if (uplodPlanFile.PlanFile is not null)
                {
                    /*
                      Get Uploaded file extension to check if the extension is:
                      .pdf for tree plan.
                      .xls/.xlsx for Study Plane.
                    */
                    string fileExtension = Path.GetExtension(uplodPlanFile.PlanFile.FileName);

                    if (planType.Equals("TreePlan") && !(fileExtension.Equals(".pdf")))
                    {
                        ModelState.AddModelError(nameof(uplodPlanFile.PlanFile), "Only pdf file extension is allow");
                        return View();
                    }

                    if (planType.Equals("StudyPlan") && (!fileExtension.Equals(".xls") && !fileExtension.Equals(".xlsx")))
                    {
                        ModelState.AddModelError(nameof(uplodPlanFile.PlanFile), @"Only xls/xlsx file extension are allow");
                        return View();
                    }

                    string planpath = $"{_webHostEnvironment.WebRootPath}\\AdvisingMaterial\\Majors";

                    /*
                     Determine whether IsActive property is true,
                     then the system shall add new foler for new major inside AdvisingMaterial in wwwroot folder.
                    */
                    if (uplodPlanFile.IsActive)
                    {
                        if (string.IsNullOrEmpty(uplodPlanFile.NewMajorName))
                        {
                            ModelState.AddModelError(nameof(uplodPlanFile.NewMajorName), "New Major Name is required");
                            return View();
                        }

                        // Combine the New Major Name to (wwwroot\AdvisingMaterial\Majors) path
                        planpath = Path.Combine(planpath, uplodPlanFile.NewMajorName);
                        if (!System.IO.Directory.Exists(planpath))
                        {
                            AdvicingMatelrialFolderMangment.CreateNewDirectory(planpath);
                        }
                    }
                    else
                    {
                        // Combine the Major Name to (wwwroot\AdvisingMaterial\Majors) path
                        planpath = Path.Combine(planpath, uplodPlanFile.MajorName);
                    }

                    planpath += $@"\{planType}";
                    if (!System.IO.Directory.Exists(planpath))
                    {
                        AdvicingMatelrialFolderMangment.CreateNewDirectory(planpath);
                    }

                    planpath = AdvicingMatelrialFolderMangment.CombineNewFileNameToPlanePath(planType, planpath, uplodPlanFile.PlanSemester);
                    AdvicingMatelrialFolderMangment.SavePlanFile(planpath, uplodPlanFile.PlanFile);
                    return View();
                }
                ModelState.AddModelError(nameof(uplodPlanFile.PlanFile), "The Plan File field is required.");
                return View();
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
