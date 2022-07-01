using Aide.Attribute;
using Aide.Data;
using Aide.Models;
using Aide.Service.ExcelSheetService;
using Aide.Service.SupuervisedInfoAPIService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aide.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IStudyPlan _studyPlan;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ISupuervisedInfoService _supuervisedInfoService;

        public HomeController(
            IConfiguration configuration,
            IStudyPlan studyPlan,
            IWebHostEnvironment webHostEnvironment,
            ISupuervisedInfoService supuervisedInfoService
            )
        {
            _configuration = configuration;
            _studyPlan = studyPlan;
            _webHostEnvironment = webHostEnvironment;
            _supuervisedInfoService = supuervisedInfoService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (Request.Cookies.Keys.Contains("cookiesession"))
                {
                    int year = DateTime.Now.Year - 1;

                    /*int month = DateTime.Now.Month;
                    int day = DateTime.Now.Day;
                    string semester = year.ToString();*/

                    /*if ((month >= 10 && month <= 1) || (month == 2 && day <= 10))
                    {
                        semester = year.ToString();
                    }
                    else if ((month >= 3 && month <= 6) || (month == 2 && day >= 15))
                    {
                        semester = year.ToString() + 2;
                    }
                    else if (month >= 7 && month <= 8)
                    {
                        semester = year.ToString() + 3;
                    }*/
                    ViewData["MajorsName"] = AdvicingMatelrialFolderMangment.GetMajorsName(_webHostEnvironment);
                    return View(new StudentPlanInfo { Year = 0 });
                }
            }
            return RedirectToAction(nameof(Login), "Accounts");
        }

        [HttpPost]
        [SessionExpiredAttribute]
        public async Task<IActionResult> Index([Bind("Year,Semester")] StudentPlanInfo model)
        {
            if (ModelState.IsValid)
            {
                if (model.Semester != 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                if (model.Year != 0)
                {
                    return RedirectToAction(nameof(Index));
                }

                Token token = CookieAttribute.GetToken(HttpContext);
                string user = CookieAttribute.GetUser(HttpContext);

                string passCode = _configuration["GetStudentinfo:passCode"];
                if (!string.IsNullOrEmpty(passCode))
                {
                    ProfessorInfo info = new ProfessorInfo { Username = user, passCode = passCode };
                    IEnumerable<Supuervised> Supuervised = _supuervisedInfoService.GetSupuervisedInfo(HttpContext, model, info);
                    if (Supuervised.Any())
                    {
                        try
                        {
                            await _studyPlan.PlanGenerator(Supuervised, user);
                        }
                        catch (Exception ex) when (ex.GetType().Name == "AuthenticationException")
                        {
                            return RedirectToAction(nameof(AccountsController.SignedOut), "Accounts");
                        }
                    }
                    else
                    {
                        TempData["Message"] = "There is no data for this account";
                    }
                }
                else
                {
                    return StatusCode(500, "HTTP ERROR 500, passCode can't be empty, Please contact with computer center to solve it");
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MajorsName"] = AdvicingMatelrialFolderMangment.GetMajorsName(_webHostEnvironment);
            return View(model);
        }
    }
}
