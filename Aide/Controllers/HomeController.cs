using Aide.Data;
using Aide.Extensions;
using Aide.Models;
using Aide.Service.ExcelSheetService;
using Aide.Service.SupuervisedInfoAPIService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Aide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private IStudyPlan _studyPlan;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ISupuervisedInfoService _supuervisedInfoService;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration,
            IStudyPlan studyPlan,
            IWebHostEnvironment webHostEnvironment,
            ISupuervisedInfoService supuervisedInfoService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _studyPlan = studyPlan;
            _webHostEnvironment = webHostEnvironment;
            _supuervisedInfoService = supuervisedInfoService;
        }

        [HttpGet]
        public IActionResult Index()
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
            return RedirectToAction(nameof(Login), "Accounts");
        }

        [HttpPost]
        [SessionExpiredAttribute]
        public async Task<IActionResult> Index(StudentPlanInfo model)
        {
            if (ModelState.IsValid)
            {
                Token token = CookiSpace.GetToken(HttpContext);
                string user = CookiSpace.GetUser(HttpContext);

                /*if (token is not null && string.IsNullOrEmpty(user))
                {*/
                string passCode = _configuration["GetStudentinfo:passCode"];
                if (!string.IsNullOrEmpty(passCode))
                {
                    ProfessorInfo info = new ProfessorInfo { Username = user, passCode = passCode };
                    IEnumerable<Supuervised> Supuervised = _supuervisedInfoService.GetSupuervisedInfo(HttpContext, model, info);
                    if (Supuervised.Any())
                    {
                        try
                        {
                            await _studyPlan.GenarateExcelSheet(Supuervised, user);
                        }
                        catch (Exception ex) when (ex.GetType().Name == "AuthenticationException")
                        {
                            return RedirectToAction(nameof(AccountsController.SignedOut), "Accounts");
                        }
                    }
                    else
                    {
                        /*ModelState.AddModelError("Supuervised_API_Error", "There is no data for this account");*/
                        TempData["SupuervisedAPIError"] = "There is no data for this account";
                    }
                }
                else
                {
                    // TODO
                    return StatusCode(500);
                }
                /*}
                else
                {
                    TempData["ErrorCause"] = "access token or email not found, Please Login again or contact Computer Center to fix this issues";
                    return RedirectToAction(nameof(Login), "Accounts");
                }*/
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
