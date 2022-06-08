using Aide.Data;
using Aide.Extensions;
using Aide.Service.SupuervisedInfoAPIService;
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
        private readonly ISupuervisedInfoService _supuervisedInfoService;

        public AccountsController(IConfiguration configuration, ISupuervisedInfoService supuervisedInfoService)
        {
            _configuration = configuration;
            _supuervisedInfoService = supuervisedInfoService;
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
                    int statusCode = _supuervisedInfoService.GetAccessToken(login, HttpContext);
                    if (statusCode == 200)
                    {
                        return RedirectToAction(nameof(SignIn));
                    }
                    /*int statusCode = (int)result.StatusCode;
                    TempData["StatusCodeError"] = result.StatusCode.ToString();*/
                    return StatusCode(statusCode);
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
