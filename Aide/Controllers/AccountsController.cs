using Aide.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Aide.Controllers
{
    public class AccountsController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Login login)
        {
            if (ModelState.IsValid)
            {
                login.grand_type = _configuration["GetToken:grand_type"];
                login.client_id = _configuration["GetToken:client_id"];
                login.scope = _configuration["GetToken:scope"];

                var dict = new Dictionary<string, string>();
                dict.Add("grant_type", login.grand_type);
                dict.Add("client_id", login.client_id);
                dict.Add("scope", login.scope);
                dict.Add("username", login.Username);
                dict.Add("password", login.Password);
                Token token1 = null;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://id.asu.edu.jo");
                    var responce = client.PostAsync("connect/token", new FormUrlEncodedContent(dict));
                    responce.Wait();
                    var result = responce.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        HttpContent content = result.Content;
                        string jsoncontent = content.ReadAsStringAsync().Result;
                        HttpContext.Session.Set("token", System.Text.Encoding.ASCII.GetBytes(jsoncontent));
                       
                        return RedirectToAction(nameof(Index), "Home");
                    }
                    else
                    {
                        return View();
                    }
                }

            }
            return View(login);
        }


    }
}
