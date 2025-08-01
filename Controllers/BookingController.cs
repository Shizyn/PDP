﻿using System.ComponentModel.DataAnnotations;
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

        public IActionResult Index(DateTime? startDate, DateTime? endDate, string status)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (userEmail == null)
                return RedirectToAction("Login", "Home");

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return NotFound();

            var bookingsQuery = _context.Bookings
                .Where(b => b.UserId == user.UserId)
                .Include(b => b.ProfProba)
                .Include(b => b.Files)
                .Include(b => b.User)
                .AsQueryable();

            var excursionBookingsQuery = _context.ExcursionBookings
                .Where(e => e.UserId == user.UserId)
                .Include(e => e.Museum)
                .Include(e => e.Files)
                .Include(b => b.User)
                .AsQueryable();

            if (startDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= startDate);
                excursionBookingsQuery = excursionBookingsQuery.Where(e => e.BookingDate >= startDate);
            }

            if (endDate.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= endDate);
                excursionBookingsQuery = excursionBookingsQuery.Where(e => e.BookingDate <= endDate);
            }

            if (!string.IsNullOrEmpty(status) && status != "Все")
            {
                bookingsQuery = bookingsQuery.Where(b => b.Status == status);
                excursionBookingsQuery = excursionBookingsQuery.Where(e => e.Status == status);
            }

            var bookings = bookingsQuery.ToList();
            var excursionBookings = excursionBookingsQuery.ToList();

            var feedbacks = _context.Feedbacks
                .Where(f => f.UserId == user.UserId)
                .ToList();

            ViewBag.Feedbacks = feedbacks;
            ViewBag.Excursions = excursionBookings;
            ViewBag.StatusFilter = status; 
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd"); 
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd"); 

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
            try
            {
                if (!DateTime.TryParse(date, out var selectedDate))
                    return BadRequest("Неверный формат даты");

                var times = _context.ExcursionSlots
                    .Where(s => s.MuseumId == museumId && s.SlotDate == selectedDate)
                    .Select(s => s.TimeRange)
                    .ToList();

                var busyTimes = _context.ExcursionBookings
                    .Where(b => b.MuseumId == museumId && b.BookingDate == selectedDate)
                    .Select(b => b.TimeRange)
                    .ToList();

                return Json(times.Except(busyTimes).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAvailableExcursionTimes");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
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
        public IActionResult CreateFeedback(int bookingId, string type)
        {
            ViewBag.BookingId = bookingId;
            ViewBag.Type = type; // "prof" или "excursion"
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback(int bookingId, string type, int rating, string text)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
                return RedirectToAction("Login", "Home");

            var feedback = new Feedback
            {
                Rating = rating,
                Text = text,
                UserId = user.UserId,
                CreatedAt = DateTime.Now
            };

            if (type == "prof")
            {
                feedback.BookingId = bookingId;
            }
            else if (type == "excursion")
            {
                feedback.ExcursionBookingId = bookingId;
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult DownloadExcelTemplate(string schoolName, int peopleCount, DateTime bookingDate, string timeRange, string eventName, string profName)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Список участников");

            int rowIndex = 1;

            worksheet.Cells[rowIndex, 1].Value = $"Информация о мероприятии";
            worksheet.Cells[rowIndex, 1].Style.Font.Bold = true;
            worksheet.Cells[rowIndex, 1].Style.Font.Size = 16;
            rowIndex += 2;

            if (!string.IsNullOrEmpty(profName))
            {
                worksheet.Cells[rowIndex, 1].Value = "Профпроба:";
                worksheet.Cells[rowIndex, 2].Value = profName;
                rowIndex++;
            }

            if (!string.IsNullOrEmpty(eventName))
            {
                worksheet.Cells[rowIndex, 1].Value = "Мероприятие:";
                worksheet.Cells[rowIndex, 2].Value = eventName;
                rowIndex++;
            }
            worksheet.Cells[rowIndex, 1].Value = "Дата проведения:";
            worksheet.Cells[rowIndex, 2].Value = bookingDate.ToString("dd.MM.yyyy");
            rowIndex++;

            worksheet.Cells[rowIndex, 1].Value = "Время проведения:";
            worksheet.Cells[rowIndex, 2].Value = timeRange;
            rowIndex++;

            worksheet.Cells[rowIndex, 1].Value = "Учебное заведение:";
            worksheet.Cells[rowIndex, 2].Value = schoolName;
            rowIndex += 2; 

            worksheet.Cells[rowIndex, 1].Value = "Контактная информация ответственного лица:";
            worksheet.Cells[rowIndex, 1].Style.Font.Bold = true;
            rowIndex++;

            if (user != null)
            {
                worksheet.Cells[rowIndex, 1].Value = "ФИО:";
                worksheet.Cells[rowIndex, 2].Value = user.FullName ?? "ФИО не указано";
                rowIndex++;


                worksheet.Cells[rowIndex, 1].Value = "Телефон:";
                worksheet.Cells[rowIndex, 2].Value = user.Phone ?? "Телефон не указан";
                rowIndex++;

                worksheet.Cells[rowIndex, 1].Value = "Email:";
                worksheet.Cells[rowIndex, 2].Value = user.Email ?? "Email не указан";
                rowIndex++;
            }
            else
            {
                worksheet.Cells[rowIndex, 1].Value = "Данные пользователя не найдены";
                rowIndex++;
            }

            rowIndex++; 

            worksheet.Cells[rowIndex, 1].Value = $"Список участников ({peopleCount} человек)";
            using (ExcelRange range = worksheet.Cells[rowIndex, 1, rowIndex, 8])
            {
                range.Merge = true;
                range.Style.Font.Bold = true;
                range.Style.Font.Size = 14;
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            rowIndex++;

            worksheet.Cells[rowIndex, 1].Value = "№";
            worksheet.Cells[rowIndex, 2].Value = "ФИО участника";
            worksheet.Cells[rowIndex, 3].Value = "Дата рождения";
            worksheet.Cells[rowIndex, 4].Value = "Школа, класс";

            worksheet.Cells[rowIndex, 6].Value = "№";
            worksheet.Cells[rowIndex, 7].Value = "ФИО сопровождающего";
            worksheet.Cells[rowIndex, 8].Value = "Должность";

            ApplyHeaderStyle(worksheet.Cells[rowIndex, 1, rowIndex, 4]);
            ApplyHeaderStyle(worksheet.Cells[rowIndex, 6, rowIndex, 8]);
            rowIndex++;

            int participantsStartRow = rowIndex;
            for (int i = 1; i <= peopleCount; i++)
            {
                worksheet.Cells[rowIndex, 1].Value = i;
                rowIndex++;
            }

            int escortsStartRow = participantsStartRow;
            for (int i = 1; i <= 4; i++)
            {
                worksheet.Cells[escortsStartRow, 6].Value = i;

                if (i == 1 && user != null)
                {
                    worksheet.Cells[escortsStartRow, 7].Value = user.FullName ?? "ФИО не указано";
                }
                escortsStartRow++;
            }

            ApplyTableStyle(worksheet.Cells[participantsStartRow, 1, participantsStartRow + peopleCount - 1, 4]);
            ApplyTableStyle(worksheet.Cells[participantsStartRow, 6, participantsStartRow + 3, 8]);

            worksheet.Cells.AutoFitColumns();

            worksheet.Column(3).Style.Numberformat.Format = "dd.mm.yyyy";

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Список_участников_{schoolName}_{bookingDate:ddMMyyyy}.xlsx"
            );
        }

        private void ApplyHeaderStyle(ExcelRange headerCells)
        {
            headerCells.Style.Font.Bold = true;
            headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            headerCells.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#BDD7EE"));
            headerCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private void ApplyTableStyle(ExcelRange tableCells)
        {
            tableCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            tableCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            tableCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            tableCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;

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
