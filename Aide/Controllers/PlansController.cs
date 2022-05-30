using Aide.Data;
using Aide.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult UplodFile(string planType)
        {
            if (!string.IsNullOrEmpty(planType))
            {
                ViewData["PlanType"] = planType;
                ViewData["MajorsName"] = AdvicingMatelrialFolderMangment.GetMajorsName(_webHostEnvironment);
                return View();
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        public IActionResult UplodFile(string planType, UplodPlanFile uplodPlanFile, IFormFile planeFile)
        {
            if (!string.IsNullOrEmpty(planType))
            {
                if (ModelState.IsValid)
                {
                    if (planeFile is not null)
                    {
                        string planepath = $"{_webHostEnvironment.WebRootPath}\\AdvisingMaterial\\Majors";
                        
                        /*if (!System.IO.Directory.Exists(planepath))
                        {
                            System.IO.Directory.CreateDirectory(planepath);
                        }*/
                        if (uplodPlanFile.IsActive)
                        {
                            planepath += uplodPlanFile.NewMajorName;
                            if (!System.IO.Directory.Exists(planepath))
                            {
                                System.IO.Directory.CreateDirectory(planepath);
                            }
                        }
                        else
                        {
                            planepath += uplodPlanFile.MajorName;
                        }

                        if (planType.Equals("TreePlane") || planType.Equals(""))
                        {
                            planepath += $@"\{planType}";
                            if (!System.IO.Directory.Exists(planepath))
                            {
                                System.IO.Directory.CreateDirectory(planepath);
                            }
                            AdvicingMatelrialFolderMangment.SavePlanFile(planepath, planeFile);
                        }
                        return View();
                    }
                }

                /*ViewData["PlanType"] = planType;
                ViewData["MajorsName"] = AdvicingMatelrialFolderMangment.GetMajorsName(_webHostEnvironment);*/
                return View();
            }
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


    }
}
