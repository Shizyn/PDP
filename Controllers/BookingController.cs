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
            {
                return RedirectToAction("Login", "Home");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound();
            }
            var excursionBookings = _context.ExcursionBookings
            .Where(e => e.UserId == user.UserId)
            .ToList();

            ViewBag.Excursions = excursionBookings;

            var bookings = _context.Bookings
         .Where(b => b.UserId == user.UserId)
         .Include(b => b.Event)
         .ThenInclude(e => e.ProfProba)
         .Include(b => b.Files)
         .ToList();

            return View(bookings);
        }

        [HttpGet]
        public IActionResult GetEventsByProfProba(int profProbaId)
        {
            var events = _context.Events
                .Where(e => e.ProfProbaId == profProbaId)
                .ToList();

            return Json(events);
        }
        [HttpGet]
        public IActionResult ExcursionCreate()
        {
            return View("ExcursionCreate");
        }

        [HttpPost]
        public IActionResult ExcursionCreate(string fullName, string email, string phoneNumber, string schoolName, DateTime bookingDate, string timeRange)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var excursion = new ExcursionBooking
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                SchoolName = schoolName,
                BookingDate = bookingDate,
                TimeRange = timeRange,
                Status = "Новое",
                UserId = user.UserId
            };

            try
            {
                _context.ExcursionBookings.Add(excursion);
                _context.SaveChanges();
                return RedirectToAction("Index", "Booking");
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Ошибка при сохранении: {errorMessage}");
                return View("ExcursionCreate");
            }
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
        public IActionResult Create(string eventType, int profProbaId, int eventId, string fullName, string email, string phoneNumber, string schoolName, DateTime bookingDate, string timeRange, int peopleCount)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            try
            {
                string typeEventValue = ""; 

                if (eventType == "MasterClass")
                {
                    typeEventValue = "Мастер-класс";
                }
                else if (eventType == "Excursion")
                {
                    typeEventValue = "Экскурсия";
                }
                else
                {
                    //если eventType не "MasterClass" и не "Excursion", добавляем ошибку в ModelState и выходим
                    ModelState.AddModelError("eventType", "Неверный тип события");
                    ViewBag.ProfProby = _context.ProfProby.ToList();
                    ViewBag.Events = _context.Events.ToList();
                    return View(); //возвращаем представление с ошибками
                }
                if (peopleCount < 1 || peopleCount > 30)
                {
                    ModelState.AddModelError("peopleCount", "Можно записать от 1 до 30 человек.");
                    ViewBag.ProfProby = _context.ProfProby.ToList();
                    ViewBag.Events = _context.Events.ToList();
                    return View();
                }
                //создание новой заявки
                var booking = new Booking
                {
                    TypeEvent = typeEventValue,
                    ProfProbaId = profProbaId,
                    EventId = eventId,
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


                _context.Bookings.Add(booking);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }   
            catch (DbUpdateException ex)
            {
                //записываем детали исключения в журнал
                _logger.LogError(ex, "Ошибка при сохранении новой заявки");
                //добавляем сообщение об ошибке в ModelState
                ModelState.AddModelError("", "Произошла ошибка при сохранении заявки. Пожалуйста, попробуйте снова");
                ViewBag.Events = _context.Events.ToList();
                ViewBag.ProfProby = _context.ProfProby.ToList();
                return View();
            }
        }
        // Получить доступные даты
        [HttpGet]
        public IActionResult GetAvailableDates(int profProbaId, int eventId)
        {
            // 1) Выбираем из БД все слоты по профпробе, событию и времени
            var rawSlots = _context.AvailableSlots
                .Where(s =>
                    s.ProfProbaId == profProbaId &&
                    s.EventID == eventId &&
                    s.TimeRange == "12:00-14:00"
                )
                .AsEnumerable(); // дальше работаем LINQ-to-Objects

            // 2) Оставляем только вторники и четверги
            var dates = rawSlots
                .Where(s =>
                    s.SlotDate.DayOfWeek == DayOfWeek.Tuesday ||
                    s.SlotDate.DayOfWeek == DayOfWeek.Thursday
                )
                .Select(s => s.SlotDate.ToString("yyyy-MM-dd"))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            return Json(dates);
        }

        public IActionResult GetAvailableTimes(int profProbaId, int eventId, DateTime date)
        {
            var bookedTimes = _context.Bookings
                .Where(b => b.ProfProbaId == profProbaId && b.EventId == eventId && b.BookingDate == date)
                .Select(b => b.TimeRange)
                .ToList();

            var times = _context.AvailableSlots
                .Where(s => s.ProfProbaId == profProbaId && s.EventID == eventId && s.SlotDate == date)
                .Select(s => s.TimeRange)
                .ToList();

            var freeTimes = times.Except(bookedTimes).ToList();
            return Json(freeTimes);
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

        [HttpGet]
        public IActionResult CreateFinalBooking()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Home");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.UserBookings = _context.Bookings
                .Where(b => b.UserId == user.UserId)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateFinalBooking(
        [Required] int bookingId,
        [Required] IFormFile excelFile)
        //,[Required] IFormFile pdfFile)
        {
            try
            {
                // Проверка существования заявки
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    TempData["Error"] = "Заявка не найдена";
                    return RedirectToAction("CreateFinalBooking");
                }

                // Сохранение файлов
                using (var excelStream = new MemoryStream())
                //using (var pdfStream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(excelStream);
                    //await pdfFile.CopyToAsync(pdfStream);

                    _context.UploadedFiles.Add(new UploadedFile
                    {
                        BookingId = bookingId,
                        FileName = excelFile.FileName,
                        FileType = "Excel",
                        Content = excelStream.ToArray()
                    });

                    //_context.UploadedFiles.Add(new UploadedFile
                    //{
                    //    BookingId = bookingId,
                    //    FileName = pdfFile.FileName,
                    //    FileType = "PDF",
                    //    Content = pdfStream.ToArray()
                    //});

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Файлы успешно прикреплены к заявке";
                return RedirectToAction("Index"); // Перенаправляем на список заявок
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении файлов");
                TempData["Error"] = $"Ошибка: {ex.Message}";
                return RedirectToAction("CreateFinalBooking");
            }
        }

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

        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _context.UploadedFiles.FindAsync(id);
            if (file == null)
            {
                return NotFound("Файл не найден!"); // Если файл не найден
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
