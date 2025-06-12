using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DP.Database;
using DP.Models;

namespace DP.Controllers
{
    public class HomeController : Controller
    {
        private readonly DPContext _context;
        public HomeController(DPContext context)
        {
            _context = context;
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserEmail");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (userEmail != null)
            {
                ViewData["UserEmail"] = userEmail;
            }
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                //сущ ли пользователь с таким email и паролем
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("UserEmail", user.Email);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult RegIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegIn(RegInModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Password = model.Password
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login", "Home");
            }

            return View(model);
        }
    }
}
