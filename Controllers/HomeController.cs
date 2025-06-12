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
            ModelState.Clear();
            return View(new RegInModel());
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

                // Если запрос через AJAX, возвращаем JSON для редиректа
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Login", "Home") });
                }
                return RedirectToAction("Login", "Home");
            }
            else
            {
                // Собираем ошибки из ModelState
                var errors = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    // Возвращаем JSON с ошибками – при этом данные формы не очищаются
                    return Json(new { success = false, errors });
                }
                // При обычной отправке возвращаем View с моделью, чтобы значения полей сохранились
                return View(model);
            }
        }

    }
}
