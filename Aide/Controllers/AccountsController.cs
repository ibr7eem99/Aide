using Aide.Data;
using Aide.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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

        #region ASU Login
        // Aide Project Login
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
                _configuration.Bind("GetToken", login);
                if (!string.IsNullOrEmpty(login.grand_type) && !string.IsNullOrEmpty(login.client_id) && !string.IsNullOrEmpty(login.scope))
                {
                    var dict = new Dictionary<string, string>();
                    dict.Add("grant_type", login.grand_type);
                    dict.Add("client_id", login.client_id);
                    dict.Add("scope", login.scope);
                    dict.Add("username", login.Username);
                    dict.Add("password", login.Password);

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
                            /*HttpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("m_albashayreh"));*/
                            HttpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("m_aloudat"));
                            /*HttpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("a_abusamaha"));*/
                            /*HttpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("y_alqasrawi"));*/
                            /*HttpContext.Session.Set("user", System.Text.Encoding.ASCII.GetBytes("w_manaseer"));*/
                            return RedirectToAction(nameof(SignIn));
                        }
                        int statusCode = (int)result.StatusCode;
                        TempData["StatusCodeError"] = result.StatusCode.ToString();
                        return StatusCode(statusCode);
                    }
                }
                else
                {
                    TempData["ErrorCause"] = "grant_type, client_id or scope value shouldn't be empty, please Contact Computer Center to fix this issues";
                    return StatusCode(500);
                }

            }
            return View(login);
        }
        #endregion

        #region Microsoft Login
        // Microsoft Login
        [HttpGet]
        public IActionResult SignIn()
        {
            var redirectUrl = Url.Action(nameof(Index), "Home");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            var callbackUrl = Url.Action(nameof(Login), "Accounts", values: null, protocol: Request.Scheme);
            CookiSpace.ClearAllCookis(Request, Response);
            return SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }
        #endregion
    }
}
