using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using FerryApp.Models;
using Microsoft.AspNetCore.Http;

namespace FerryApp.Controllers
{
    public class TicketController : Controller
    {
        private readonly ILogger<TicketController> _logger;
        private readonly ACE42023Context db;

        public TicketController(ACE42023Context db, ILogger<TicketController> logger) {
            this.db = db;
            _logger = logger;
        }

        public IActionResult Index() {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");
            
            string name = HttpContext.Request.Query["name"];
            string origin = HttpContext.Request.Query["origin"];
            string destination = HttpContext.Request.Query["destination"];
            string ferry = HttpContext.Request.Query["ferry"];
            string departureStr = HttpContext.Request.Query["departure"];
            string minCostStr = HttpContext.Request.Query["min_cost"];
            string maxCostStr = HttpContext.Request.Query["max_cost"];

            var result = db.ManasTickets.OrderBy(ticket => ticket.Ferry.Departure).ToList();
            if(name != null && name.Length > 0) {
                name = name.ToLower().Trim();
                result = result.Where(ticket => ticket.User.Name.ToLower().Contains(name)).ToList();
            }
            if(origin != null && origin.Length > 0) {
                origin = origin.ToLower().Trim();
                result = result.Where(ticket => ticket.Ferry.Origin.Name.ToLower().Contains(origin)).ToList();
            }
            if(destination != null && destination.Length > 0) {
                destination = destination.ToLower().Trim();
                result = result.Where(ticket => ticket.Ferry.Destination.Name.ToLower().Contains(destination)).ToList();
            }
            if(ferry != null && ferry.Length > 0) {
                ferry = ferry.ToLower().Trim();
                result = result.Where(ticket => ticket.Ferry.Name.ToLower().Contains(ferry)).ToList();
            }
            if(departureStr != null && departureStr.Length > 0) {
                DateTime departure = DateTime.Parse(departureStr); 
                result = result.Where(ticket => ticket.Ferry.Departure >= departure).ToList();
            }
            if(minCostStr != null && minCostStr.Length > 0) {
                float minCost = float.Parse(minCostStr);
                result = result.Where(ticket => ticket.Cost >= minCost).ToList();
            }
            if(maxCostStr != null && maxCostStr.Length > 0) {
                float maxCost = float.Parse(maxCostStr);
                result = result.Where(ticket => ticket.Cost <= maxCost).ToList();
            }
            return View(result);
        }

        [HttpPost]
        public IActionResult Search(IFormCollection formCollection) {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");
            
            string name = formCollection["name"].ToString();
            string origin = formCollection["origin"].ToString();
            string destination = formCollection["destination"].ToString();
            string ferry = formCollection["ferry"].ToString();
            string departure = formCollection["departure"].ToString();
            string minCost = formCollection["min_cost"].ToString();
            string maxCost = formCollection["max_cost"].ToString();

            return RedirectToAction("Index", new {
                name = name, 
                origin = origin,
                destination = destination,
                ferry = ferry,
                departure = departure,
                min_cost = minCost,
                max_cost = maxCost
            });
        }

        public IActionResult Create(int id) {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");

            string userId = HttpContext.Session.Get("id").ToString();

            var ferries = db.ManasFerries.ToList();
            ViewBag.Ferries = new SelectList(ferries, "Id", "Name", id);
            var users = db.ManasUsers.ToList();
            ViewBag.Users = new SelectList(users, "Id", "UserName", userId);
            return View();
        }

        [HttpPost]
        public IActionResult Create([Bind(include: "AdultCount, UserId, FerryId")] ManasTicket manasTicket) {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");

            ManasUser user = db.ManasUsers.Find(manasTicket.UserId);
            ManasFerry ferry = db.ManasFerries.Find(manasTicket.FerryId);
            if(user == null || ferry == null)
                return RedirectToAction("Index", "Ferry");
            double chargePerAdult = ferry.Charge;
            manasTicket.Cost = chargePerAdult * manasTicket.AdultCount;
            if(user.Wallet < manasTicket.Cost)
                return RedirectToAction("Index", "Ferry");
            int lastRoomNoAvailable = ferry.RoomsLeft;
            manasTicket.RoomNo = lastRoomNoAvailable;
            db.ManasTickets.Add(manasTicket);
            ferry.RoomsLeft -= 1;
            db.ManasFerries.Update(ferry);
            user.Wallet -= manasTicket.Cost;
            db.ManasUsers.Update(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Route("Show/{id:int}")]
        public IActionResult Show(int id) {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");

            ManasTicket manasTicket = db.ManasTickets.Find(id);
            return View(manasTicket); 
        }

        [Route("Update/{id:int}")]
        public IActionResult Update(int id) {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");
            
            ManasTicket manasTicket = db.ManasTickets.Find(id);

            string userId = HttpContext.Session.Get("id").ToString();

            var ferries = db.ManasFerries.ToList();
            ViewBag.Ferries = new SelectList(ferries, "Id", "Name", manasTicket.FerryId);
            var users = db.ManasUsers.ToList();
            ViewBag.Users = new SelectList(users, "Id", "UserName", manasTicket.UserId);
            return View(manasTicket);
        }

        [HttpPost]
        [Route("Update/{id:int}")]
        public IActionResult Update(ManasTicket manasTicket) {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");
        
            ManasTicket ticket = db.ManasTickets.Find(manasTicket.Id);
            ManasFerry ferry = db.ManasFerries.Find(manasTicket.FerryId);
            ManasUser user = db.ManasUsers.Find(manasTicket.UserId);
            if(ticket == null || ferry == null || user == null)
                return RedirectToAction("Index");

            int prevAdultCount = ticket.AdultCount;
            int adultCount = manasTicket.AdultCount;
            int countChange = adultCount - prevAdultCount;
            double costChange = countChange * ferry.Charge;

            if(user.Wallet < costChange)
                return RedirectToAction("Index");

            ticket.AdultCount = adultCount;
            ticket.Cost = adultCount * ferry.Charge;
            user.Wallet -= costChange;

            db.ManasTickets.Update(ticket);
            db.ManasUsers.Update(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [Route("Delete/{id:int}")]
        [HttpGet]
        public IActionResult Delete(int id) {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");
            ManasTicket manasTicket = db.ManasTickets.Find(id);
            return View(manasTicket);
        }

        [HttpPost]
        [ActionName("Delete")]
        [Route("Delete/{id:int}")]
        public IActionResult DeleteConfirmed(int id) {
            byte[] value;
            bool res = HttpContext.Session.TryGetValue("id", out value);
            if(res == false)
                return RedirectToAction("Login", "Auth");
            
            ManasTicket manasTicket = db.ManasTickets.Find(id);
            ManasUser user = db.ManasUsers.Find(manasTicket.UserId);
            user.Wallet += 0.9 * manasTicket.Cost;
            db.ManasTickets.Remove(manasTicket);
            db.ManasUsers.Update(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
