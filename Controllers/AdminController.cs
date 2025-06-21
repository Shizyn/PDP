using System.Drawing;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using DP.Database;
using DP.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        //public class AdminDashboardViewModel
        //{
        //    public List<Booking> Bookings { get; set; }
        //    public List<ExcursionBooking> Excursions { get; set; }
        //    public List<Booking> FinalBookings { get; set; }
        //    public List<Feedback> Feedbacks { get; set; }
        //}
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
            {
                return RedirectToAction("Login");
            }
            var bookings = _context.Bookings
                .Include(b => b.ProfProba)

                .ToList();

            var feedbacks = _context.Feedbacks
        .Include(f => f.Booking)
            .ThenInclude(b => b.ProfProba)
        .Include(f => f.ExcursionBooking)
            .ThenInclude(e => e.Museum)
        .Include(f => f.User)
        .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                Bookings = GetBookings(),
                Excursions = GetExcursionBookings(),
                Feedbacks = feedbacks 
            };

            return View(viewModel);
        }

        private List<Booking> GetBookings()
        {
            return _context.Bookings
                .Include(b => b.ProfProba)
                .Include(b => b.User)
                .Include(b => b.Files)
                .ToList();
        }

        private List<ExcursionBooking> GetExcursionBookings()
        {
            return _context.ExcursionBookings
                .Include(e => e.Museum)
                .Include(e => e.User)
                .Include(e => e.Files)
                .ToList();
        }

        [HttpGet]
        public ActionResult AddSchedule()
        {
            var vm = new AddScheduleViewModel
            {
                Proba = _context.ProfProby
                    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                    .ToList(),
                Excursions = _context.Museums
                    .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
                    .ToList(),
                Date = DateTime.Today
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSchedule(AddScheduleViewModel vm)
        {
            vm.Proba = _context.ProfProby
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();
            vm.Excursions = _context.Museums
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
                .ToList();

            if (vm.ToTime <= vm.FromTime)
            {
                ModelState.AddModelError("", "Время окончания должно быть позже времени начала");
                return View(vm);
            }

            var timeRange = $"{vm.FromTime.Hours:00}:{vm.FromTime.Minutes:00}-{vm.ToTime.Hours:00}:{vm.ToTime.Minutes:00}";
            var date = vm.Date.Date;

            try
            {
                bool slotExists = false;

                if (vm.Type == SlotType.Профпроба)
                {
                    slotExists = _context.AvailableSlots
                        .Any(s => s.ProfProbaId == vm.SelectedEventId &&
                                  s.SlotDate == date &&
                                  s.TimeRange == timeRange);
                }
                else if (vm.Type == SlotType.Экскурсия)
                {
                    slotExists = _context.ExcursionSlots
                        .Any(s => s.MuseumId == vm.SelectedEventId &&
                                  s.SlotDate == date &&
                                  s.TimeRange == timeRange);
                }

                if (!slotExists)
                {
                    if (vm.Type == SlotType.Профпроба)
                    {
                        var slot = new AvailableSlot
                        {
                            ProfProbaId = vm.SelectedEventId,
                            SlotDate = date,
                            TimeRange = timeRange
                        };
                        _context.AvailableSlots.Add(slot);
                    }
                    else if (vm.Type == SlotType.Экскурсия)
                    {
                        var slot = new ExcursionSlot
                        {
                            MuseumId = vm.SelectedEventId,
                            SlotDate = date,
                            TimeRange = timeRange
                        };
                        _context.ExcursionSlots.Add(slot);
                    }

                    _context.SaveChanges();

                    TempData["Success"] = "Слот успешно добавлен!";
                    return RedirectToAction("AddSchedule");
                }
                else
                {
                    ModelState.AddModelError("", "Слот на это время уже существует");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении слота");
                ModelState.AddModelError("", $"Ошибка: {ex.InnerException?.Message ?? ex.Message}");
            }

            return View(vm);
        }

        [HttpGet]
        public IActionResult ManageProfProby()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            var profProby = _context.ProfProby
                .Include(p => p.Events)
                .ToList();

            var viewModel = new ProfProbaManagementViewModel
            {
                ProfProby = profProby,
                EventsByProfProba = profProby.ToDictionary(p => p.Id, p => p.Events.ToList())
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddProfProba(ProfProbaManagementViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.NewProfProbaName))
            {
                TempData["Error"] = "Название профпробы не может быть пустым";
                return RedirectToAction("ManageProfProby");
            }

            try
            {
                var profProba = new ProfProba { Name = model.NewProfProbaName };
                _context.ProfProby.Add(profProba);
                _context.SaveChanges();
                TempData["Success"] = "Профпроба успешно добавлена!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ошибка при добавлении профпробы: " + ex.Message;
            }

            return RedirectToAction("ManageProfProby");
        }

        [HttpPost]
        public IActionResult AddEvent(ProfProbaManagementViewModel model)
        {
            if (!model.SelectedProfProbaId.HasValue)
            {
                TempData["Error"] = "Выберите профессиональную пробу";
                return RedirectToAction("ManageProfProby");
            }

            if (string.IsNullOrWhiteSpace(model.NewEventName))
            {
                TempData["Error"] = "Название события не может быть пустым";
                return RedirectToAction("ManageProfProby");
            }

            try
            {
                var newEvent = new Event
                {
                    Name = model.NewEventName,
                    Description = model.NewEventDescription,
                    ProfProbaId = model.SelectedProfProbaId.Value
                };

                _context.Events.Add(newEvent);
                _context.SaveChanges();
                TempData["Success"] = "Событие успешно добавлено!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ошибка при добавлении события: " + ex.Message;
            }

            return RedirectToAction("ManageProfProby");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfProba(int id)
        {
            try
            {
                var profProba = await _context.ProfProby
                    .Include(p => p.Events)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (profProba == null)
                {
                    TempData["Error"] = "Профпроба не найдена";
                    return RedirectToAction("ManageProfProby");
                }

                _context.Events.RemoveRange(profProba.Events);
                _context.ProfProby.Remove(profProba);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Профпроба '{profProba.Name}' и все связанные события успешно удалены!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении профпробы: {ex.Message}";
                _logger.LogError(ex, "Ошибка при удалении профпробы ID: {Id}", id);
            }

            return RedirectToAction("ManageProfProby");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    TempData["Error"] = "Событие не найдено";
                    return RedirectToAction("ManageProfProby");
                }

                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Событие '{eventItem.Name}' успешно удалено!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при удалении события: {ex.Message}";
                _logger.LogError(ex, "Ошибка при удалении события ID: {Id}", id);
            }

            return RedirectToAction("ManageProfProby");
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
                excursion.Status = "Подтверждено"; 
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
            var booking = _context.Bookings
                .Include(b => b.Files)
                .FirstOrDefault(b => b.ID == id);

            if (booking != null)
            {
                // Удаляем связанные файлы
                foreach (var file in booking.Files.ToList())
                {
                    _context.UploadedFiles.Remove(file);
                }

                _context.Bookings.Remove(booking);
                _context.SaveChanges();
                TempData["Success"] = "Заявка успешно удалена!";
            }
            else
            {
                TempData["Error"] = "Заявка не найдена";
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult DeleteExcursion(int id)
        {
            var excursion = _context.ExcursionBookings
                .Include(e => e.Files)
                .FirstOrDefault(e => e.Id == id);

            if (excursion != null)
            {
                foreach (var file in excursion.Files.ToList())
                {
                    _context.ExcursionUploadedFiles.Remove(file);
                }

                _context.ExcursionBookings.Remove(excursion);
                _context.SaveChanges();
                TempData["Success"] = "Экскурсия успешно удалена!";
            }
            else
            {
                TempData["Error"] = "Экскурсия не найдена";
            }
            return RedirectToAction("Index");
        }
        // Метод для удаления окончательной заявки
        //[HttpPost]
        //public IActionResult DeleteFinalBooking(int id)
        //{
        //    var booking = _context.Bookings.Include(b => b.Files).FirstOrDefault(b => b.ID == id);
        //    if (booking != null)
        //    {
        //        Удаляем все связанные файлы
        //        if (booking.Files != null && booking.Files.Any())
        //        {
        //            _context.UploadedFiles.RemoveRange(booking.Files);
        //        }

        //        Удаляем саму заявку
        //        _context.Bookings.Remove(booking);
        //        _context.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}

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

        [HttpGet]
        public IActionResult UpdateFinalBooking(int id)
        {
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
        [HttpPost]
        public IActionResult CompleteBooking(int id)
        {
            var booking = _context.Bookings.FirstOrDefault(b => b.ID == id);
            if (booking != null && booking.Status == "Подтверждено")
            {
                booking.Status = "Завершено"; 
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CompleteExcursion(int id)
        {
            var excursion = _context.ExcursionBookings.FirstOrDefault(e => e.Id == id);
            if (excursion != null && excursion.Status == "Подтверждено")
            {
                excursion.Status = "Завершено";
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
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
            //booking.FullName = model.FullName;
            //booking.Email = model.Email;
            //booking.PhoneNumber = model.PhoneNumber;
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
                var existingFiles = await _context.UploadedFiles.Where(f => f.BookingId == bookingId).ToListAsync();
                if (existingFiles.Any())
                {
                    _context.UploadedFiles.RemoveRange(existingFiles);
                }

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
                .Include(b => b.ProfProba)
                .Include(b => b.Files)
                .ToList();

            var excursions = _context.ExcursionBookings
                .Include(e => e.User)
                .ToList();
            var model = new AdminDashboardViewModel
            {
                Bookings = _context.Bookings.Include(b => b.Files).ToList(),
                Excursions = _context.ExcursionBookings.Include(e => e.Files).ToList(),

            };
            ViewBag.Bookings = bookings;
            ViewBag.Excursions = excursions;

            return View();
        }
        public ActionResult DownloadFile(int id)
        {
            var file = _context.UploadedFiles
                .Include(f => f.Booking)
                .FirstOrDefault(f => f.Id == id);

            if (file == null)
            {
                return View();
            }

            return File(file.Content, System.Net.Mime.MediaTypeNames.Application.Octet, file.FileName);
        }
        public ActionResult ExDownloadFile(int id)
        {
            var file = _context.ExcursionUploadedFiles
                .Include(f => f.ExcursionBooking)
                .FirstOrDefault(f => f.Id == id);

            if (file == null)
            {
                return View();
            }

            return File(file.Content, System.Net.Mime.MediaTypeNames.Application.Octet, file.FileName);
        }
    }
}