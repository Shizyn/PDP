using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using DP.Database;
using DP.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DP.Controllers
{
    public class BookingController : Controller
    {
        private readonly DPContext _context;
        private readonly ILogger<BookingController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public class AdminDashboardViewModel
        {
            public List<Booking> Bookings { get; set; }
            public List<ExcursionBooking> Excursions { get; set; }
            public List<Booking> FinalBookings { get; set; }
        }

        public BookingController(DPContext context, ILogger<BookingController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
                return RedirectToAction("Login", "Home");

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return NotFound();

            var bookings = _context.Bookings
                .Where(b => b.UserId == user.UserId)
                .Include(b => b.ProfProba)
                .Include(b => b.Files) 
                .ToList();
            ViewBag.Bookings = bookings;
           
            var excursionBookings = _context.ExcursionBookings
            .Where(e => e.UserId == user.UserId)
            .Include(e => e.Files) 
            .ToList();
            ViewBag.Excursions = excursionBookings;

            return View(bookings);
        }

        [HttpGet]
        public IActionResult GetEventsByProfProba(int profProbaId)
        {
            var events = _context.Events
                .Where(e => e.ProfProbaId == profProbaId)
                .Select(e => new { e.ID, e.Name })
                .ToList();

            return Json(events);
        }
        [HttpGet]
        public IActionResult GetEventDetails(int eventId)
        {
            var ev = _context.Events.FirstOrDefault(e => e.ID == eventId);
            if (ev == null)
                return NotFound();

            return Json(new
            {
                name = ev.Name,
                description = ev.Description,
            });
        }
        public JsonResult GetAvailableTimes(int eventId, DateTime date)
        {
            var times = _context.AvailableSlots
                .Where(s => s.ProfProbaId == eventId && s.SlotDate == date)
                .Select(s => s.TimeRange)
                .ToList();

            return Json(times);
        }
        [HttpGet]
        public IActionResult GetAvailableDates(int profProbaId)
        {
            var allSlots = _context.AvailableSlots
                .Where(s => s.ProfProbaId == profProbaId)
                .ToList();

            var bookedSlots = _context.Bookings
                .Where(b =>
                    b.ProfProbaId == profProbaId)
                .Select(b => new { Date = b.BookingDate, Time = b.TimeRange })
                .ToList();

            var freeDates = allSlots
                .GroupBy(s => s.SlotDate.Date)
                .Where(g =>
                {
                    return g.Any(slot =>
                        !bookedSlots.Any(bs =>
                            bs.Date == slot.SlotDate.Date &&
                            bs.Time == slot.TimeRange));
                })
                .Select(g => g.Key.ToString("yyyy-MM-dd"))
                .ToList();

            return Json(freeDates);
        }
        [HttpGet]
        public IActionResult GetAvailableTimes(int profProbaId, string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var targetDate))
            {
                return BadRequest("Неверный формат даты");
            }

            var dailySlots = _context.AvailableSlots
                .Where(s =>
                    s.ProfProbaId == profProbaId &&
                    s.SlotDate.Date == targetDate.Date)
                .ToList();

            var bookedSlots = _context.Bookings
                .Where(b =>
                    b.ProfProbaId == profProbaId &&
                    b.BookingDate.Date == targetDate.Date)
                .Select(b => b.TimeRange)
                .ToList();

            var freeTimes = dailySlots
                .Where(s => !bookedSlots.Contains(s.TimeRange))
                .OrderBy(s => s.TimeRange) 
                .Select(s => s.TimeRange)
                .Distinct()
                .ToList();

            return Json(freeTimes);
        }

        [HttpGet]
        public IActionResult ExcursionCreate()
        {
            return View("ExcursionCreate");
        }

        [HttpPost]
        public async Task<IActionResult> ExcursionCreate(string fullName, string email, string phoneNumber, string schoolName, DateTime bookingDate, string timeRange, int peopleCount, IFormFile excelFile)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            

            // Проверяем, что выбранный слот действительно есть в таблице ExcursionSlot
            var slotExists = _context.ExcursionSlots.Any(s =>
                s.SlotDate.Date == bookingDate.Date &&
                s.TimeRange == timeRange);

            if (!slotExists)
            {
                ModelState.AddModelError("", "Выбранный временной интервал недоступен.");
                return View("ExcursionCreate");
            }

            // Проверяем занятость: если уже есть запись на эту дату и время
            var isSlotTaken = _context.ExcursionBookings.Any(e =>
                e.BookingDate.Date == bookingDate.Date &&
                e.TimeRange == timeRange);

            if (isSlotTaken)
            {
                ModelState.AddModelError("", "На выбранную дату и время уже есть заявка. Пожалуйста, выберите другой слот.");
                return View("ExcursionCreate");
            }

            var excursion = new ExcursionBooking
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                SchoolName = schoolName,
                BookingDate = bookingDate,
                TimeRange = timeRange,
                PeopleCount = peopleCount,
                Status = "Новое",
                UserId = user.UserId
            };
            try
            {
                _context.ExcursionBookings.Add(excursion);
                await _context.SaveChangesAsync();

                if (excelFile != null && excelFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await excelFile.CopyToAsync(ms);
                    var bytes = ms.ToArray();

                    var uploaded = new ExcursionUploadedFile
                    {
                        ExcursionBookingId = excursion.Id,
                        FileName = excelFile.FileName,
                        FileType = "Excel",
                        Content = bytes
                    };

                    _context.ExcursionUploadedFiles.Add(uploaded);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index", "Booking");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Ошибка при сохранении ExcursionBooking или ExcursionUploadedFile");
                var detailed = dbEx.InnerException?.Message ?? dbEx.Message;
                ModelState.AddModelError("", $"Ошибка при сохранении в БД: {detailed}");

                return View("ExcursionCreate");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка в методе ExcursionCreate");
                ModelState.AddModelError("", $"Непредвиденная ошибка: {ex.Message}");
                return View("ExcursionCreate");
            }
        }
        [HttpGet]
        public IActionResult GetAvailableExcursionDates()
        {
            var freeSlotsQuery = _context.ExcursionSlots
                .Where(slot => !_context.ExcursionBookings.Any(b =>
                    b.BookingDate.Date == slot.SlotDate.Date &&
                    b.TimeRange == slot.TimeRange));

            var freeDates = freeSlotsQuery
                .Select(slot => slot.SlotDate.Date)
                .Distinct()
                .ToList();

            var result = freeDates
                .Select(d => d.ToString("yyyy-MM-dd"))
                .ToList();

            return Json(result);
        }

        [HttpGet]
        public IActionResult GetAvailableExcursionTimes(string date)
        {
            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var targetDate))
            {
                return BadRequest("Неверный формат даты");
            }

            var dailySlots = _context.ExcursionSlots
                .Where(s => s.SlotDate.Date == targetDate.Date)
                .Select(s => s.TimeRange)
                .Distinct()
                .ToList();

            var bookedTimes = _context.ExcursionBookings
                .Where(e => e.BookingDate.Date == targetDate.Date)
                .Select(e => e.TimeRange)
                .ToList();

            var freeTimes = dailySlots
                .Where(tr => !bookedTimes.Contains(tr))
                .OrderBy(tr => tr)
                .ToList();

            return Json(freeTimes);
        }
        [HttpGet]
        public IActionResult DownloadExcursionFile(int id)
        {
            var file = _context.ExcursionUploadedFiles.FirstOrDefault(f => f.Id == id);
            if (file == null)
                return NotFound();

            var contentType = file.FileType == "Excel"
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : "application/octet-stream";

            return File(file.Content, contentType, file.FileName);
        }
        [HttpGet]
        public IActionResult Create(string eventType)
        {
            ViewBag.EventType = eventType;
            ViewBag.ProfProby = _context.ProfProby.ToList();
            ViewBag.Events = _context.Events.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int profProbaId, string fullName, string email, string phoneNumber, string schoolName, DateTime bookingDate, string timeRange, int peopleCount, IFormFile excelFile)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(schoolName) ||
                string.IsNullOrWhiteSpace(timeRange))
                {
                    ModelState.AddModelError("", "Все поля обязательны.");
                    ViewBag.ProfProby = _context.ProfProby.ToList();
                    ViewBag.Events = _context.Events.ToList();
                    return View();
                }

                // Проверяем наличие занятого слота
                var isSlotTaken = _context.Bookings.Any(b =>
                    b.ProfProbaId == profProbaId &&
                    b.BookingDate.Date == bookingDate.Date &&
                    b.TimeRange == timeRange);

                if (isSlotTaken)
                {
                    ModelState.AddModelError("", "На выбранную дату и время уже есть заявка. Пожалуйста, выберите другой слот.");
                    ViewBag.ProfProby = _context.ProfProby.ToList();
                    return View();
                }
                //создание новой заявки
                var booking = new Booking
                {
                    ProfProbaId = profProbaId,
                    FullName = fullName,
                    //BirthDate = birthDate,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    SchoolName = schoolName,
                    BookingDate = bookingDate,
                    TimeRange = timeRange,
                    PeopleCount = peopleCount,
                    Status = "Новое",
                    UserId = user.UserId

                };

                try
                {
                    // Сохраняем Booking, чтобы получить его Id
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    if (excelFile != null && excelFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await excelFile.CopyToAsync(ms);
                        var bytes = ms.ToArray();

                        var uploaded = new UploadedFile
                        {
                            BookingId = booking.ID, 
                            FileName = excelFile.FileName,
                            FileType = "Excel",
                            Content = bytes
                        };

                        _context.UploadedFiles.Add(uploaded);
                        await _context.SaveChangesAsync();
                    }

                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Ошибка при сохранении новой заявки");
                    ModelState.AddModelError("", "Произошла ошибка при сохранении заявки. Пожалуйста, попробуйте снова.");
                    ViewBag.ProfProby = _context.ProfProby.ToList();
                    return View();
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении новой заявки");
                ModelState.AddModelError("", "Произошла ошибка при сохранении заявки. Пожалуйста, попробуйте снова");
                ViewBag.ProfProby = _context.ProfProby.ToList();
                return View();
            }
        }

        [HttpGet]
        public IActionResult DownloadExcelTemplate()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Список участников");

            //стиль для заголовка "Список участников" (одиночная ячейка)
            using (ExcelRange range = worksheet.Cells[1, 1])
            {
                range.Value = "Список участников";
                range.Style.Font.Bold = true;
                range.Style.Font.Size = 12;
            }

            //заголовки для участников
            worksheet.Cells[2, 1].Value = "№";
            worksheet.Cells[2, 2].Value = "ФИО";
            worksheet.Cells[2, 3].Value = "Дата рождения";
            worksheet.Cells[2, 4].Value = "Школа, класс";

            //применяем стиль к заголовкам
            string headerParticipantsRange = "A2:D2";
            ApplyHeaderStyle(worksheet.Cells[headerParticipantsRange]);

            //заполнение 18 строк (нумерация)
            for (int i = 3; i <= 20; i++)
            {
                worksheet.Cells[i, 1].Value = i - 2; //номер
            }

            //устанавливаем границы для таблицы участников
            string tableParticipantsRange = "A2:D20";
            ApplyTableStyle(worksheet.Cells[tableParticipantsRange]);

            //заголовки для сопровождающих
            worksheet.Cells[22, 1].Value = "Сопровождающие";
            using (ExcelRange range = worksheet.Cells[22, 1])
            {
                range.Style.Font.Bold = true;
                range.Style.Font.Size = 12;
            }

            worksheet.Cells[23, 1].Value = "№";
            worksheet.Cells[23, 2].Value = "ФИО";
            worksheet.Cells[23, 3].Value = "Дата рождения";
            worksheet.Cells[23, 4].Value = "Должность";

            //применяем стиль к заголовкам сопровождающих
            string headerAccompanyingRange = "A23:D23";
            ApplyHeaderStyle(worksheet.Cells[headerAccompanyingRange]);

            //заполнение 4 строк (нумерация)
            for (int i = 24; i <= 27; i++)
            {
                worksheet.Cells[i, 1].Value = i - 23; //номер
            }

            //применяем стиль к таблице сопровождающих
            string tableAccompanyingRange = "A23:D27";
            ApplyTableStyle(worksheet.Cells[tableAccompanyingRange]);

            //автоматическая настройка ширины столбцов
            worksheet.Cells.AutoFitColumns();

            //возвращаем файл
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Шаблон_участников.xlsx");
        }

        private void ApplyHeaderStyle(ExcelRange headerCells)
        {
            headerCells.Style.Font.Bold = true; //шрифт жирный
            headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid; //тип заливки
            headerCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#BDD7EE")); //цвет заливки
            headerCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;  //выравнивание по центру
        }

        private void ApplyTableStyle(ExcelRange tableCells)
        {
            tableCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            tableCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            tableCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            tableCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //цвет границ
            tableCells.Style.Border.Top.Color.SetColor(Color.Black);
            tableCells.Style.Border.Bottom.Color.SetColor(Color.Black);
            tableCells.Style.Border.Left.Color.SetColor(Color.Black);
            tableCells.Style.Border.Right.Color.SetColor(Color.Black);
        }

        //[HttpGet]
        //public IActionResult CreateFinalBooking()
        //{
        //    var userEmail = HttpContext.Session.GetString("UserEmail");
        //    if (string.IsNullOrEmpty(userEmail))
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    ViewBag.UserBookings = _context.Bookings
        //        .Where(b => b.UserId == user.UserId)
        //        .OrderByDescending(b => b.BookingDate)
        //        .ToList();

        //    return View();
        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateFinalBooking(
        //[Required] int bookingId,
        //[Required] IFormFile excelFile)
        ////,[Required] IFormFile pdfFile)
        //{
        //    try
        //    {
        //        // Проверка существования заявки
        //        var booking = await _context.Bookings.FindAsync(bookingId);
        //        if (booking == null)
        //        {
        //            TempData["Error"] = "Заявка не найдена";
        //            return RedirectToAction("CreateFinalBooking");
        //        }

        //        // Сохранение файлов
        //        using (var excelStream = new MemoryStream())
        //        //using (var pdfStream = new MemoryStream())
        //        {
        //            await excelFile.CopyToAsync(excelStream);
        //            //await pdfFile.CopyToAsync(pdfStream);

        //            _context.UploadedFiles.Add(new UploadedFile
        //            {
        //                BookingId = bookingId,
        //                FileName = excelFile.FileName,
        //                FileType = "Excel",
        //                Content = excelStream.ToArray()
        //            });

        //            //_context.UploadedFiles.Add(new UploadedFile
        //            //{
        //            //    BookingId = bookingId,
        //            //    FileName = pdfFile.FileName,
        //            //    FileType = "PDF",
        //            //    Content = pdfStream.ToArray()
        //            //});

        //            await _context.SaveChangesAsync();
        //        }

        //        TempData["Success"] = "Файлы успешно прикреплены к заявке";
        //        return RedirectToAction("Index"); // Перенаправляем на список заявок
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Ошибка при сохранении файлов");
        //        TempData["Error"] = $"Ошибка: {ex.Message}";
        //        return RedirectToAction("CreateFinalBooking");
        //    }
        //}

        //[HttpGet("DownloadApplicationLetter")]
        //public IActionResult DownloadApplicationLetter()
        //{
        //    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Заявка.pdf");

        //    if (!System.IO.File.Exists(filePath))
        //        return NotFound("Файл не найден!");

        //    // Открываем PDF в браузере (не скачиваем)
        //    Response.Headers.Add("Content-Disposition", "inline; filename=Заявка.pdf");
        //    return PhysicalFile(filePath, "application/pdf");
        //}

        //[HttpGet("DownloadFile")]
        //public async Task<IActionResult> DownloadFile(int id)
        //{
        //    var file = await _context.UploadedFiles.FindAsync(id);
        //    if (file == null)
        //    {
        //        return NotFound("Файл не найден!"); // Если файл не найден
        //    }

        //    // Определяем тип контента в зависимости от типа файла
        //    var contentType = file.FileType switch
        //    {
        //        "Excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "PDF" => "application/pdf",
        //        _ => "application/octet-stream" // Для других типов файлов
        //    };

        //    // Возвращаем файл с правильным именем
        //    return File(file.Content, contentType, file.FileName);
        //}
    }
}
