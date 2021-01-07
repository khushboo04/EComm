﻿using EComm.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EComm.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("login")]
        public IActionResult Login(string ReturnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = ReturnUrl });
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel lvm)
        {
            if (!ModelState.IsValid) return View(lvm);

            bool auth = (lvm.Username == "test" && lvm.Password == "password");

            if (!auth)
            {
                ModelState.AddModelError(string.Empty, "Invalid Login");
                return View(lvm);
            }

            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new List<Claim> {
                    new Claim(ClaimTypes.Name, lvm.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                }, CookieAuthenticationDefaults.AuthenticationScheme));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (lvm.ReturnUrl != null) return LocalRedirect(lvm.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
