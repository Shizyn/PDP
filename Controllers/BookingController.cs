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
                .Include(b => b.User)
                .ToList();
            ViewBag.Bookings = bookings;
           
            var excursionBookings = _context.ExcursionBookings
            .Where(e => e.UserId == user.UserId)
            .Include(e => e.Museum)
            .Include(e => e.Files)
            .Include(b => b.User)
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

        [HttpGet]
        public JsonResult GetAvailableDates(int profProbaId)
        {
            var availableDates = _context.AvailableSlots
                .Where(s => s.ProfProbaId == profProbaId)
                .Select(s => s.SlotDate)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            var formattedDates = availableDates.Select(d => d.ToString("yyyy-MM-dd")).ToList();

            return Json(formattedDates);
        }
        [HttpGet]
        public JsonResult GetAvailableTimes(int profProbaId, DateTime date)
        {
            var slots = _context.AvailableSlots
                .Where(s => s.ProfProbaId == profProbaId && s.SlotDate == date.Date)
                .Select(s => s.TimeRange)
                .ToList();

            // Получаем уже занятые слоты
            var busyTimes = _context.Bookings
                .Where(b => b.BookingDate == date && b.ProfProbaId == profProbaId)
                .Select(b => b.TimeRange)
                .ToList();

            var availableTimes = slots.Except(busyTimes).ToList();

            return Json(availableTimes);
        }

        [HttpGet]
        public IActionResult ExcursionCreate()
        {
            ViewBag.Museums = _context.Museums.ToList(); 
            return View("ExcursionCreate");
        }
        [HttpPost]
        public async Task<IActionResult> ExcursionCreate(int museumId, string schoolName, DateTime bookingDate, string timeRange, int peopleCount, IFormFile excelFile)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                if (string.IsNullOrWhiteSpace(schoolName) || string.IsNullOrWhiteSpace(timeRange) || museumId == 0 || bookingDate == default)
                {
                    ModelState.AddModelError("", "Все поля обязательны.");
                    ViewBag.Museums = _context.Museums.ToList();
                    return View("ExcursionCreate");
                }

                int totalEvents = _context.ExcursionBookings
                    .Count(e => e.BookingDate.Date == bookingDate.Date && e.TimeRange == timeRange)
                    + _context.Bookings
                    .Count(p => p.BookingDate.Date == bookingDate.Date && p.TimeRange == timeRange);

                if (totalEvents >= 3)
                {
                    ModelState.AddModelError("", "На выбранную дату и время уже запланировано максимальное количество мероприятий (3). Пожалуйста, выберите другой слот.");
                    ViewBag.Museums = _context.Museums.ToList();
                    return View("ExcursionCreate");
                }

                var isSlotTaken = _context.ExcursionBookings.Any(e =>
                    e.MuseumId == museumId &&
                    e.BookingDate.Date == bookingDate.Date &&
                    e.TimeRange == timeRange);

                if (isSlotTaken)
                {
                    ModelState.AddModelError("", "На выбранную дату и время уже есть заявка. Пожалуйста, выберите другой слот.");
                    ViewBag.Museums = _context.Museums.ToList();
                    return View("ExcursionCreate");
                }

                var excursionBooking = new ExcursionBooking
                {
                    MuseumId = museumId,
                    SchoolName = schoolName,
                    BookingDate = bookingDate,
                    TimeRange = timeRange,
                    PeopleCount = peopleCount,
                    Status = "Новое",
                    UserId = user.UserId
                };

                _context.ExcursionBookings.Add(excursionBooking);
                await _context.SaveChangesAsync();

                if (excelFile != null && excelFile.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await excelFile.CopyToAsync(ms);
                    var bytes = ms.ToArray();

                    var uploaded = new ExcursionUploadedFile
                    {
                        ExcursionBookingId = excursionBooking.Id,
                        FileName = excelFile.FileName,
                        FileType = "Excel",
                        Content = bytes
                    };

                    _context.ExcursionUploadedFiles.Add(uploaded);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Ошибка при сохранении новой экскурсионной заявки: {ex.InnerException?.Message ?? ex.Message}");
                ModelState.AddModelError("", $"Ошибка при сохранении в БД: {ex.InnerException?.Message ?? ex.Message}");
                ViewBag.Museums = _context.Museums.ToList();
                return View("ExcursionCreate");
            }
        }

        [HttpGet]
        public IActionResult GetAvailableExcursionDates(int museumId)
        {
            var dates = _context.ExcursionSlots
                .Where(s => s.MuseumId == museumId)
                .Select(s => s.SlotDate)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            return Json(dates.Select(d => d.ToString("yyyy-MM-dd")).ToList());
        }

        [HttpGet]
        public IActionResult GetAvailableExcursionTimes(int museumId, string date)
        {
            if (!DateTime.TryParse(date, out var selectedDate))
                return BadRequest();

            var times = _context.ExcursionSlots
                .Where(s => s.MuseumId == museumId && s.SlotDate == selectedDate)
                .Select(s => s.TimeRange)
                .ToList();

            var busyTimes = _context.ExcursionBookings
                .Where(b => b.BookingDate == selectedDate)
                .Select(b => b.TimeRange)
                .ToList();

            return Json(times.Except(busyTimes).ToList());
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
        [HttpPost]
        public async Task<IActionResult> Create(int profProbaId, string schoolName, DateTime bookingDate, string timeRange, int peopleCount, IFormFile excelFile)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                if (string.IsNullOrWhiteSpace(schoolName) ||
                    string.IsNullOrWhiteSpace(timeRange) ||
                    profProbaId == 0 ||
                    bookingDate == default)
                {
                    ModelState.AddModelError("", "Все поля обязательны.");
                    ViewBag.ProfProby = _context.ProfProby.ToList();
                    ViewBag.Events = _context.Events.ToList();
                    return View();
                }

                // Проверка, занят ли слот
                var isSlotTaken = _context.Bookings.Any(b =>
                    b.ProfProbaId == profProbaId &&
                    b.BookingDate.Date == bookingDate.Date &&
                    b.TimeRange == timeRange);

                if (isSlotTaken)
                {
                    ModelState.AddModelError("", "На выбранную дату и время уже есть заявка. Пожалуйста, выберите другой слот.");
                    ViewBag.ProfProby = _context.ProfProby.ToList();
                    ViewBag.Events = _context.Events.ToList();
                    return View();
                }

                var booking = new Booking
                {
                    ProfProbaId = profProbaId,
                    SchoolName = schoolName,
                    BookingDate = bookingDate,
                    TimeRange = timeRange,
                    PeopleCount = peopleCount,
                    Status = "Новое",
                    UserId = user.UserId
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Сохраняем файл Excel
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
                var detailed = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "Ошибка при сохранении новой заявки: " + detailed);
                ModelState.AddModelError("", $"Ошибка при сохранении в БД: {detailed}");
                ViewBag.ProfProby = _context.ProfProby.ToList();
                ViewBag.Events = _context.Events.ToList();
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
