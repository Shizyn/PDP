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
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
                    HttpContext.Session.SetString("UserEmail", user.Email);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Неверный email или пароль" });
                    }
                    ModelState.AddModelError(string.Empty, "Неверный email или пароль");
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckEmailAvailable(string email)
        {
            bool emailExists = _context.Users.Any(u => u.Email == email);
            return Json(new { isAvailable = !emailExists });
        }

        [HttpGet]
        public IActionResult RegIn()
        {
            ModelState.Clear();
            return View(new RegInModel());
        }

        [HttpPost]
        public IActionResult RegIn(RegInModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    return Json(new
                    {
                        success = false,
                        errors = new Dictionary<string, List<string>> {
                    { "Email", new List<string> { "Этот email уже зарегистрирован" } }
                        }
                    });
                }
                var user = new User
                {
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Password = model.Password
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Login", "Home") });
                }
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, errors });
                }
                return View(model);
            }
        }

    }
}
