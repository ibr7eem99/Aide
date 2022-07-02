using Aide.Attribute;
using Aide.Data;
using Microsoft.AspNetCore.Hosting;
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
        [Route("[controller]/[action]/{planType?}")]
        public IActionResult UploadFile(string planType)
        {
            // Check if the value of planType is null Or empty it should redirect to home page
            if (!string.IsNullOrEmpty(planType))
            {
                // Check if the value of planType is not equal to StudyPlan or TreePlan it should redirect to home page
                if (planType.Equals("StudyPlan") || planType.Equals("TreePlan"))
                {
                    // save plan type in ViewData to use it in the url in the _Layout view inside shared folder
                    /*ViewData["PlanType"] = planType;*/
                    // get the majors name and save it in ViewData it used to display the majors in _SidebarPartial inside shared folder
                    ViewData["MajorsName"] = AdvicingMatelrialFolderMangment.GetMajorsName(_webHostEnvironment);
                    ViewData["User"] = CookieAttribute.GetUser(HttpContext);
                    return View();
                }
                else
                {
                    // save the error message in TempData to display it in home view if the value of planType is not equal to StudyPlan or TreePlan
                    TempData["Message"] = "Plan Type value should be only StudyPlan or TreePlan";
                }
            }
            else
            {
                // save the error message in TempData to display it in home view if the value of planType is empty
                TempData["Message"] = "Plan Type value can't be empty";
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [Route("[controller]/[action]/{planType?}")]
        [ValidateAntiForgeryToken]
        public IActionResult UploadFile(string planType, [Bind("PlanYear,PlanFile,MajorName,IsActive,NewMajorName")] UplodPlanFile uplodPlanFile)
        {
            if (!string.IsNullOrEmpty(planType))
            {
                if (ModelState.IsValid)
                {
                    // Get Plan Type (TreePlane or StudyPlane) to use it in _Layout shared view
                    /*ViewData["PlanType"] = planType;*/
                    // Get Majors Name from wwwroot folder and use it in _Layout shared view to show the tree plan for each major
                    ViewData["MajorsName"] = AdvicingMatelrialFolderMangment.GetMajorsName(_webHostEnvironment);
                    ViewData["User"] = CookieAttribute.GetUser(HttpContext);
                    /*
                      Get Uploaded file extension to check if the extension is:
                      .pdf for tree plan.
                      .xlsx for Study Plane.
                    */
                    string fileExtension = Path.GetExtension(uplodPlanFile.PlanFile.FileName);

                    if (planType.Equals("TreePlan") && !(fileExtension.Equals(".pdf")))
                    {
                        ModelState.AddModelError(nameof(uplodPlanFile.PlanFile), "Only pdf file extension is allow");
                        return View();
                    }

                    if (planType.Equals("StudyPlan") && (!fileExtension.Equals(".xlsx")))
                    {
                        ModelState.AddModelError(nameof(uplodPlanFile.PlanFile), @"Only xlsx file extension are allow");
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
                        if (string.IsNullOrEmpty(uplodPlanFile.MajorName))
                        {
                            ModelState.AddModelError(nameof(uplodPlanFile.NewMajorName), "Major name is required");
                            return View();
                        }
                        // Combine the Major Name to (wwwroot\AdvisingMaterial\Majors) path
                        planpath = Path.Combine(planpath, uplodPlanFile.MajorName);
                    }

                    planpath += $@"\{planType}";
                    if (!System.IO.Directory.Exists(planpath))
                    {
                        AdvicingMatelrialFolderMangment.CreateNewDirectory(planpath);
                    }

                    planpath = AdvicingMatelrialFolderMangment.CombineNewFileNameToPlanePath(planType, planpath, uplodPlanFile.PlanYear);
                    AdvicingMatelrialFolderMangment.SavePlanFile(planpath, uplodPlanFile.PlanFile);
                    TempData["Message"] = "The file was uploaded successfully";
                    return RedirectToAction(nameof(UploadFile), new { planType = planType });
                }
                else
                {
                    return View(uplodPlanFile);
                }
            }
            TempData["Message"] = "Plan Type value could not be empty";
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        public IActionResult DeleteFile(string fileName, string major)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["Message"] = "File name could not be empty";
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            if (string.IsNullOrEmpty(major))
            {
                TempData["Message"] = "Major could not be empty";
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            string filePath = $"{_webHostEnvironment.WebRootPath}\\AdvisingMaterial\\Majors\\{major}\\TreePlan\\{fileName}";
            if (System.IO.File.Exists(filePath))
            {
                TempData["Message"] = "File deleted success";
                System.IO.File.Delete(filePath);
            }
            else
            {
                TempData["Message"] = "No file found";
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
