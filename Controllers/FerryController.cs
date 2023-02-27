using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FerryApp.Models;
using Microsoft.AspNetCore.Http;

namespace FerryApp.Controllers
{
    public class FerryController : Controller
    {
        private readonly ILogger<FerryController> _logger;
        private readonly ACE42023Context db;

        public FerryController(ACE42023Context db, ILogger<FerryController> logger)
        {
            this.db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            string origin = HttpContext.Request.Query["origin"];
            string destination = HttpContext.Request.Query["destination"];
            string name = HttpContext.Request.Query["name"];
            string departureStr = HttpContext.Request.Query["departure"];
            string minChargeStr = HttpContext.Request.Query["min_charge"];
            string maxChargeStr = HttpContext.Request.Query["max_charge"];

            var result = db.ManasFerries.OrderBy(ferry => ferry.Departure).ToList();
            if(origin != null && origin.Length > 0) {
                origin = origin.ToLower().Trim();
                result = result.Where(ferry => ferry.Origin.Name.ToLower().Contains(origin)).ToList();
            }
            if(destination != null && destination.Length > 0) {
                destination = destination.ToLower().Trim();
                result = result.Where(ferry => ferry.Destination.Name.ToLower().Contains(destination)).ToList();
            }
            if(name != null && name.Length > 0) {
                name = name.ToLower().Trim();
                result = result.Where(ferry => ferry.Name.ToLower().Contains(name)).ToList();
            }
            if(departureStr != null && departureStr.Length > 0) {
                DateTime departure = DateTime.Parse(departureStr); 
                result = result.Where(ferry => ferry.Departure >= departure).ToList();
            }
            if(minChargeStr != null && minChargeStr.Length > 0) {
                float minCharge = float.Parse(minChargeStr);
                result = result.Where(ferry => ferry.Charge >= minCharge).ToList();
            }
            if(maxChargeStr != null && maxChargeStr.Length > 0) {
                float maxCharge = float.Parse(maxChargeStr);
                result = result.Where(ferry => ferry.Charge <= maxCharge).ToList();
            }
            return View(result);
        }

        [HttpPost]
        public IActionResult Search(IFormCollection formCollection) {
            string origin = formCollection["origin"].ToString();
            string destination = formCollection["destination"].ToString();
            string name = formCollection["name"].ToString();
            string departure = formCollection["departure"].ToString();
            string minCharge = formCollection["min_charge"].ToString();
            string maxCharge = formCollection["max_charge"].ToString();
            return RedirectToAction("Index", new {
                origin = origin,
                destination = destination,
                name = name, 
                departure = departure,
                min_charge = minCharge,
                max_charge = maxCharge
            });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
