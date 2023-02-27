using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using FerryApp.Models;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace firstmvcapp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ACE42023Context db;
        private readonly ISession session;
        public AuthController(ACE42023Context _db, IHttpContextAccessor httpContextAccessor)
        {
            db = _db;
            session = httpContextAccessor.HttpContext.Session;
        }

        public IActionResult Login() {
            return View();
        }

        [HttpPost]
        public IActionResult Login([Bind(include: "UserName, Password")] ManasUser manasUser) {
            var result = db.ManasUsers.Where(user => user.UserName == manasUser.UserName && user.Password == manasUser.Password).SingleOrDefault();
            if(result == null)
                return View();
            HttpContext.Session.Set("id", System.Text.Encoding.UTF8.GetBytes(result.Id.ToString()));
            HttpContext.Session.Set("username", System.Text.Encoding.UTF8.GetBytes(result.UserName));
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Logout() {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Profile() {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");
            
            int id = Convert.ToInt16(HttpContext.Session.GetString("id"));
            ManasUser manasUser = db.ManasUsers.Find(id);
            return View(manasUser);
        }
    }
}