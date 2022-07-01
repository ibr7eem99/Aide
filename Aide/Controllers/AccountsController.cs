using Aide.Attribute;
using Aide.Data;
using Aide.Service.SupuervisedInfoAPIService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;

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
        [ValidateAntiForgeryToken]
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
                    else if (statusCode == 400)
                    {
                        ModelState.AddModelError("InvalidAuthentication", "Invalid Username or Password, Please try again.");
                        return View(nameof(Login));
                    }
                }
                else
                {
                    return StatusCode(500, "Status Code: 500; grant_type, client_id or scope value shouldn't be empty, please Contact Computer Center to fix this issues");
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
            CookieAttribute.ClearAllCookis(Request, Response);
            return SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }
        #endregion
    }
}
