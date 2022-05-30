using Aide.Data;
using Aide.Extensions;
using Aide.Models;
using Aide.Service.ExcelSheetService;
using Aide.Service.GraphAPIService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Aide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private IStudyPlan _studyPlan;
        private readonly IGraphServiceClientFactory _graphServiceClientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration,
            IStudyPlan studyPlan,
            IGraphServiceClientFactory graphServiceClientFactory,
            IWebHostEnvironment webHostEnvironment
            )
        {
            _logger = logger;
            _configuration = configuration;
            _studyPlan = studyPlan;
            _graphServiceClientFactory = graphServiceClientFactory;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Index(StudentPlanInfo model)
        {
            if (ModelState.IsValid)
            {
                IEnumerable<Supuervised> Supuervised = null;
                Token token = CookiSpace.GetToken(HttpContext);
                string user = CookiSpace.GetUser(HttpContext);

                if (token is not null && user is not null)
                {
                    if (!string.IsNullOrEmpty(_configuration["GetStudentinfo:passCode"]))
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("https://api.asu.edu.jo/");
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.token_type, token.access_token);
                            int semester = (int)model.Semester;
                            /*ProfessorInfo professor = new ProfessorInfo { Username = "w_manaseer", passCode = _configuration["GetStudentinfo:passCode"] };*/
                            ProfessorInfo professor = new ProfessorInfo { Username = "m_aloudat", passCode = _configuration["GetStudentinfo:passCode"] };
                            /*ProfessorInfo professor = new ProfessorInfo { Username = "a_abusamaha", passCode = _configuration["GetStudentinfo:passCode"] };*/
                            /*ProfessorInfo professor = new ProfessorInfo { Username = "m_albashayreh", passCode = _configuration["GetStudentinfo:passCode"] };*/
                            /*ProfessorInfo professor = new ProfessorInfo { Username = "y_alqasrawi", passCode = _configuration["GetStudentinfo:passCode"] };*/
                            var responce = client.PostAsJsonAsync($"api/Courses/Supervisored?year={model.Year}&semester={semester}", professor);
                            responce.Wait();
                            var result = responce.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                HttpContent httpContent = result.Content;
                                string jsoncontent = httpContent.ReadAsStringAsync().Result;
                                Supuervised = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Supuervised>>(jsoncontent);
                                var graphClient = _graphServiceClientFactory.GetAuthenticatedGraphClient((ClaimsIdentity)User.Identity);
                                try
                                {
                                    await _studyPlan.GenarateExcelSheet(Supuervised, user, graphClient);
                                }
                                catch (Exception ex) when (ex.GetType().Name == "AuthenticationException")
                                {
                                    return RedirectToAction(nameof(AccountsController.SignedOut), "Accounts");
                                }
                            }
                            else
                            {
                                Supuervised = Enumerable.Empty<Supuervised>();
                            }
                        }
                    }
                    else
                    {
                        // TODO
                        return StatusCode(500);
                    }
                }
                else
                {
                    TempData["ErrorCause"] = "access token or email not found, Please Login again or contact Computer Center to fix this issues";
                    throw new UnauthorizedAccessException();
                }
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
