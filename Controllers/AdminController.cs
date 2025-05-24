using System.Drawing;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using DP.Database;
using DP.Models;

namespace DP.Controllers
{
    public class AdminController : Controller
    {
        private readonly DPContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(DPContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public class AdminDashboardViewModel
        {
            public List<Booking> Bookings { get; set; }
            public List<ExcursionBooking> Excursions { get; set; }
            public List<Booking> FinalBookings { get; set; }
        }
        public IActionResult Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.ProfProba)
                .Include(b => b.Event)
                .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                Bookings = GetBookings(),
                Excursions = GetExcursionBookings(),
                FinalBookings = GetFinalBookings()
            };

            return View(viewModel);
        }

        private List<Booking> GetBookings()
        {
            return _context.Bookings.ToList();
        }

        private List<ExcursionBooking> GetExcursionBookings()
        {
            return _context.ExcursionBookings.ToList();
        }

        private List<Booking> GetFinalBookings()
{
    return _context.Bookings
        .Include(b => b.ProfProba)
        .Include(b => b.Event)
        .Include(b => b.Files)
        .Include(b => b.User)
        .Where(b => b.Files.Any()) // Фильтруем по наличию файлов
        .ToList();
}
        public IActionResult Excursions()
        {
            if (HttpContext.Session.GetString("IsAdmin") == "true")
            {
                var excursions = _context.ExcursionBookings.Include(e => e.User).ToList();
                return View(excursions);
            }

            return RedirectToAction("Login");
        }
        [HttpPost]
        public IActionResult DeleteExcursion(int id)
        {
            var excursion = _context.ExcursionBookings.FirstOrDefault(e => e.Id == id);
            if (excursion == null) return NotFound();

            _context.ExcursionBookings.Remove(excursion);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult EditExcursion(int id)
        {
            var excursion = _context.ExcursionBookings.FirstOrDefault(e => e.Id == id);
            if (excursion == null)
            {
                return NotFound();
            }
            return View(excursion);
        }


        [HttpPost]
        public IActionResult EditExcursion(ExcursionBooking updated)
        {
            if (ModelState.IsValid)
            {
                _context.ExcursionBookings.Update(updated);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(updated);
        }

        [HttpPost]
        public IActionResult ConfirmExcursion(int id)
        {
            var excursion = _context.ExcursionBookings.FirstOrDefault(e => e.Id == id);
            if (excursion != null)
            {
                excursion.Status = "Подтверждена";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejectExcursion(int id)
        {
            var excursion = _context.ExcursionBookings.FirstOrDefault(e => e.Id == id);
            if (excursion != null)
            {
                excursion.Status = "Отклонена";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult UpdateStatus(int bookingId, string status)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.ID == bookingId);

            if (booking != null && booking.Status == "Новое")
            {
                booking.Status = status;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.ID == id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Метод для удаления окончательной заявки
        [HttpPost]
        public IActionResult DeleteFinalBooking(int id)
        {
            var booking = _context.Bookings.Include(b => b.Files).FirstOrDefault(b => b.ID == id);
            if (booking != null)
            {
                // Удаляем все связанные файлы
                if (booking.Files != null && booking.Files.Any())
                {
                    _context.UploadedFiles.RemoveRange(booking.Files);
                }

                // Удаляем саму заявку
                _context.Bookings.Remove(booking);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Confirm(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.ID == id);
            if (booking != null && booking.Status == "Новое")
            {
                booking.Status = "Подтверждено";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.ID == id);
            if (booking != null && booking.Status == "Новое")
            {
                booking.Status = "Отклонено";
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "admin")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Неверный логин или пароль");
            return View();
        }
       
       

        // Метод для редактирования окончательной заявки
        [HttpGet]
        public IActionResult UpdateFinalBooking(int id)
        {
            //var booking = _context.Bookings.FirstOrDefault(b => b.ID == id);
            var booking = _context.Bookings.Include(b => b.Files).FirstOrDefault(b => b.ID == id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
        [HttpGet]
        public IActionResult Update(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.ProfProba)
                .Include(b => b.Event)
                .FirstOrDefault(b => b.ID == id);

            if (booking == null)
            {
                return NotFound();
            }

            ViewBag.ProfProby = _context.ProfProby.ToList();
            ViewBag.Events = _context.Events
                .Where(e => e.ProfProbaId == booking.ProfProbaId)
                .ToList();

            return View(booking);
        }

        [HttpGet]
        public IActionResult GetEventsByProfProba(int profProbaId)
        {
            var events = _context.Events
                .Where(e => e.ProfProbaId == profProbaId)
                .Select(e => new { id = e.ID, name = e.Name })
                .ToList();

            return Json(events);
        }

        [HttpPost]
        public IActionResult Update(int id, Booking model)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Обновляем только разрешенные поля
            booking.ProfProbaId = model.ProfProbaId;
            booking.EventId = model.EventId;
            booking.FullName = model.FullName;
            booking.Email = model.Email;
            booking.PhoneNumber = model.PhoneNumber;
            booking.SchoolName = model.SchoolName;
            booking.BookingDate = model.BookingDate;
            booking.TimeRange = model.TimeRange;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFinalBooking(int bookingId, IFormFile excelFile, IFormFile pdfFile)
        {
            var booking = await _context.Bookings.Include(b => b.Files).FirstOrDefaultAsync(b => b.ID == bookingId);
            if (booking == null)
            {
                TempData["Error"] = "Заявка не найдена.";
                return RedirectToAction("Index");
            }

            try
            {
                // Удаляем все прикрепленные файлы для данной заявки
                var existingFiles = await _context.UploadedFiles.Where(f => f.BookingId == bookingId).ToListAsync();
                if (existingFiles.Any())
                {
                    _context.UploadedFiles.RemoveRange(existingFiles);
                }

                // Сохранение файлов, если они были загружены
                if (excelFile != null)
                {
                    using (var excelStream = new MemoryStream())
                    {
                        await excelFile.CopyToAsync(excelStream);
                        _context.UploadedFiles.Add(new UploadedFile
                        {
                            BookingId = bookingId,
                            FileName = excelFile.FileName,
                            FileType = "Excel",
                            Content = excelStream.ToArray()
                        });
                    }
                }

                if (pdfFile != null)
                {
                    using (var pdfStream = new MemoryStream())
                    {
                        await pdfFile.CopyToAsync(pdfStream);
                        _context.UploadedFiles.Add(new UploadedFile
                        {
                            BookingId = bookingId,
                            FileName = pdfFile.FileName,
                            FileType = "PDF",
                            Content = pdfStream.ToArray()
                        });
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Данные успешно обновлены.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении данных.");
                TempData["Error"] = "Ошибка при обновлении данных.";
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult AllBookings()
        {
            var bookings = _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.ProfProba)
                .Include(b => b.Files)
                .Include(b => b.User)
                .ToList();

            var excursions = _context.ExcursionBookings
                .Include(e => e.User)
                .ToList();

            ViewBag.Bookings = bookings;
            ViewBag.Excursions = excursions;

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _context.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id);

            if (file == null || file.Content == null)
            {
                return NotFound(); // Если файл не найден
            }

            // Определяем тип контента в зависимости от типа файла
            var contentType = file.FileType switch
            {
                "Excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "PDF" => "application/pdf",
                _ => "application/octet-stream" // Для других типов файлов
            };

            // Возвращаем файл с правильным именем
            return File(file.Content, contentType, file.FileName);
        }
    }
}
