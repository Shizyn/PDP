using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DP.Controllers.BookingController;

namespace DP.Models
{
    public class Booking
    {
        [Key]
        public int ID { get; set; }

        // Ссылка на профпробу
        [ForeignKey("ProfProba")]
        [Required]
        public int ProfProbaId { get; set; }
        public ProfProba ProfProba { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^\+7 \(\d{3}\) \d{3}-\d{2}-\d{2}$")]
        public string PhoneNumber { get; set; }

        [Required]
        public string SchoolName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required]
        public string TimeRange { get; set; }

        [Required]
        public string Status { get; set; } = "Новое";

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 30, ErrorMessage = "Количество человек должно быть от 1 до 30")]
        public int PeopleCount { get; set; } = 1;

        // Навигация к файлам
        public ICollection<UploadedFile> Files { get; set; } = new List<UploadedFile>();
    }
}
