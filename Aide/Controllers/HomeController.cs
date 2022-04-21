using Aide.Data;
using Aide.Models;
using Aide.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Aide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Now.Day;
            string semester = year.ToString() + 1;


            if ((month >= 10 && month <= 1) || (month == 2 && day <= 10))
            {
                semester = year.ToString() + 1;
            }
            else if ((month >= 3 && month <= 6) || (month == 2 && day >= 15))
            {
                semester = year.ToString() + 2;
            }
            else if (month >= 7 && month <= 8)
            {
                semester = year.ToString() + 3;
            }

            return View(new StudentPlanInfo { Year = Convert.ToInt32(semester) });
        }

        /*[HttpPost]
        public IActionResult Index(StudentPlanInfo model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }*/

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
