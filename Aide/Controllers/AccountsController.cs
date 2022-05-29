﻿using Aide.Data;
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
        private readonly IHttpClientFactory _clientFactory;

        public AccountsController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

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
                login.grand_type = _configuration["GetToken:grand_type"];
                login.client_id = _configuration["GetToken:client_id"];
                login.scope = _configuration["GetToken:scope"];

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
                        /*return RedirectToAction(nameof(Index), "Home");*/
                        return RedirectToAction(nameof(SignIn));
                    }
                    else
                    {
                        return View();
                    }
                }

            }
            return View(login);
        }

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
    }
}
