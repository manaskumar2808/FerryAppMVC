using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FerryApp.Models;

namespace FerryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ACE42023Context db;

        public HomeController(ACE42023Context db, ILogger<HomeController> logger)
        {
            this.db = db;
            _logger = logger;
        }

        public IActionResult Index() {
            return View(db.ManasPorts);
        }

        public IActionResult About() {
            return View();
        }

        public IActionResult Contact() {
            return View();
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
