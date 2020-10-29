using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using H5Serv.Models;
using H5Serv.Models.DB;
using H5Serv.Data;
using BC = BCrypt.Net.BCrypt;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;

namespace H5Serv.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _context;
        private readonly IDataProtector _provider;
        private readonly IConfiguration _config;

        public HomeController(ILogger<HomeController> logger, MyDbContext context, IDataProtectionProvider provider, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _config = config;
            _provider = provider.CreateProtector(_config["SecretKey"]);
            
        }

        public IActionResult Index()
        {
            var FlashMessage = HttpContext.Session.GetString("FlashMessage");
            if(FlashMessage != null)
            {
                ViewBag.FlashMessage = FlashMessage;
                HttpContext.Session.Remove("FlashMessage");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            var userID = HttpContext.Session.GetInt32("UserId");
            if(userID == null)
            {
                return Redirect("/Home/login");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            user.Password = BC.HashPassword(user.Password);

            user.Email = _provider.Protect(user.Username);

            _context.User.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetString("FlashMessage", "Brugeren blev oprettet");
            return Redirect("/Home/Login");

            //return View();
        }

        [HttpGet]
        async public Task<IActionResult> TodoItem()
        {
            
            var userID = HttpContext.Session.GetInt32("UserId");
            if (userID == null)
            {
                return Redirect("/Home/login");
            }
            ViewBag.User = await _context.User.Include(u => u.TodoItems).FirstOrDefaultAsync(u => u.Id == userID);

            foreach (TodoItem todo in ViewBag.User.TodoItems)
            {
                try
                {
                    todo.Title = _provider.Unprotect(todo.Title);
                    todo.Description = _provider.Unprotect(todo.Description);
                }
                catch (Exception ex)
                {

                }
            }
            return View();
        }

        [HttpPost]
        public IActionResult TodoItem(TodoItem todoItem)
        {
            var userID = HttpContext.Session.GetInt32("UserId");
            if (userID == null)
            {
                return Redirect("/Home/login");
            }
            string userIdInt = userID.ToString();
            todoItem.UserId = Int32.Parse(userIdInt);
            todoItem.Title = _provider.Protect(todoItem.Title);
            todoItem.Description = _provider.Protect(todoItem.Description);

            _context.TodoItem.Add(todoItem);
            _context.SaveChanges();

            HttpContext.Session.SetString("FlashMessage", "Noten blev oprettet");
            return Redirect("/Home/TodoItem");
        }





        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string username, string password)
        {

            if(username == null || password == null)
            {
                ViewBag.Message = "Udfyld venligst begge felter";
                return View();
            }

            string email = _provider.Protect(username);
            var user = _context.User.FirstOrDefault(u => u.Username == username || u.Email == email);


            if (user == null)
            {
                ViewBag.Message = "Ukendt brugernavn";
                return View();
            }

            if(BC.Verify(password, user.Password))
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                return Redirect("/Home/Privacy");
            }
            else
            {
                ViewBag.Message = "Forkert password";
                return View();

            }

        }



        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/Home/Login");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        //test
    }
}
