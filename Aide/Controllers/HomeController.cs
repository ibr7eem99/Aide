using Aide.Data;
using Aide.Extensions;
using Aide.Models;
using Aide.Service.ExcelSheetService;
using Aide.Service.GraphAPIService;
using Aide.Service.OneDriveService;
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
        private IOneDriveService _oneDriveService;
        private readonly IGraphServiceClientFactory _graphServiceClientFactory;


        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration,
            IStudyPlan studyPlan,
            IOneDriveService oneDriveService,
            IGraphServiceClientFactory graphServiceClientFactory
            )
        {
            _logger = logger;
            _configuration = configuration;
            _studyPlan = studyPlan;
            _oneDriveService = oneDriveService;
            _graphServiceClientFactory = graphServiceClientFactory;
        }

        /*[HttpGet]
        public async Task<IActionResult> Test()
        {
            if (User.Identity.IsAuthenticated)
            {
                var graphClient = _graphServiceClientFactory.GetAuthenticatedGraphClient((ClaimsIdentity)User.Identity);
                try
                {
                    DriveItem professorFolder = await _oneDriveService.GetProfessorFolder(graphClient) as DriveItem;
                    DriveItem studentFolder = await _oneDriveService.GetStudentFolder(graphClient, professorFolder.Id, "Ibraheem Fatayer 201810116") as DriveItem;
                }
                catch
                {

                }
                *//*var graphClient = _graphServiceClientFactory.GetAuthenticatedGraphClient((ClaimsIdentity)User.Identity);
                string jsonString = await GraphService.GetAllItemsInsideDrive(graphClient, HttpContext);
                try
                {
                    IEnumerable<DriveItem> folderInfo = null;
                    folderInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<DriveItem>>(jsonString);
                    ViewData["Response"] = folderInfo;
                }
                catch (Exception ex)
                {
                    ExceptionMessage message = null;
                    message = Newtonsoft.Json.JsonConvert.DeserializeObject<ExceptionMessage>(jsonString);
                }*//*
            }

            return View();
        }*/

        [HttpGet]
        public IActionResult Index()
        {
            byte[] tokenbyts = null;

            if (HttpContext.Session.TryGetValue("token", out tokenbyts))
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
                Token token = null;
                token = CookiSpace.GetToken(HttpContext);
                string user= CookiSpace.GetUser(HttpContext);
                if (token is not null && user is not null)
                {
                    if (!string.IsNullOrEmpty(_configuration["GetStudentinfo:passCode"]))
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("https://api.asu.edu.jo/");
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.token_type, token.access_token);
                            int semester = (int)model.Semester;
                            /*ProfessorInfo professor = new ProfessorInfo { Username = "m_aloudat", passCode = _configuration["GetStudentinfo:passCode"] };*/
                            /*ProfessorInfo professor = new ProfessorInfo { Username = "a_abusamaha", passCode = _configuration["GetStudentinfo:passCode"] };*/
                            /*ProfessorInfo professor = new ProfessorInfo { Username = "m_albashayreh", passCode = _configuration["GetStudentinfo:passCode"] };*/
                            ProfessorInfo professor = new ProfessorInfo { Username = "y_alqasrawi", passCode = _configuration["GetStudentinfo:passCode"] };
                            var responce = client.PostAsJsonAsync($"api/Courses/Supervisored?year={model.Year}&semester={semester}", professor);
                            responce.Wait();
                            var result = responce.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                HttpContent httpContent = result.Content;
                                string jsoncontent = httpContent.ReadAsStringAsync().Result;
                                Supuervised = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Supuervised>>(jsoncontent);
                                var graphClient = _graphServiceClientFactory.GetAuthenticatedGraphClient((ClaimsIdentity)User.Identity);
                                await _studyPlan.GenarateExcelSheet(Supuervised, user, graphClient);
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
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
