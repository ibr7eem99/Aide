using Aide.Data;
using Aide.Models;
using Aide.Service;
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
using System.Text.Json;
using System.Threading.Tasks;

namespace Aide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private IStudyPlan _studyPlan;

        private readonly IGraphServiceClientFactory _graphServiceClientFactory;

        public HomeController(
            ILogger<HomeController> logger,
            IConfiguration configuration,
            IStudyPlan studyPlan,
            IGraphServiceClientFactory graphServiceClientFactory
            )
        {
            _logger = logger;
            _configuration = configuration;
            _studyPlan = studyPlan;
            _graphServiceClientFactory = graphServiceClientFactory;
        }

        /*[HttpGet]
        public async Task<IActionResult> Test(string email)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Get users's email.
                email ??= User.FindFirst("preferred_username")?.Value;
                ViewData["Email"] = email;

                // Initialize the GraphServiceClient.
                var graphClient = _graphServiceClientFactory.GetAuthenticatedGraphClient((ClaimsIdentity)User.Identity);

                ViewData["Response"] = await GraphService.GetUserJson(graphClient, email, HttpContext);

                ViewData["Picture"] = await GraphService.GetPictureBase64(graphClient, email, HttpContext);
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
                if (ConvertJsonStringToObject() is not null)
                {
                    if (!string.IsNullOrEmpty(_configuration["GetStudentinfo:passCode"]))
                    {
                        token = ConvertJsonStringToObject();
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri("https://api.asu.edu.jo/");
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.token_type, token.access_token);
                            int semester = (int)model.Semester;
                            ProfessorInfo professor = new ProfessorInfo { Username = "m_aloudat", passCode = _configuration["GetStudentinfo:passCode"] };
                            var responce = client.PostAsJsonAsync($"api/Courses/Supervisored?year={model.Year}&semester={semester}", professor);
                            responce.Wait();
                            var result = responce.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                HttpContent httpContent = result.Content;
                                string jsoncontent = httpContent.ReadAsStringAsync().Result;
                                Supuervised = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Supuervised>>(jsoncontent);
                                await _studyPlan.GenarateExcelSheet(Supuervised);
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

        private Token ConvertJsonStringToObject()
        {
            byte[] tokenbyts = null;
            string tokenvalue = null;
            if (HttpContext.Session.TryGetValue("token", out tokenbyts))
            {
                tokenvalue = System.Text.Encoding.ASCII.GetString(tokenbyts);
                return JsonSerializer.Deserialize<Token>(tokenvalue);
            }
            return null;
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
