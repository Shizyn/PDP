using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DP.Models
{
    public class ExcursionBooking
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ФИО обязательно")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Телефон обязателен")]
        [RegularExpression(@"^\+7 \(\d{3}\) \d{3}-\d{2}-\d{2}$",
            ErrorMessage = "Телефон должен быть в формате +7 (999) 999-99-99")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Школа обязательна")]
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "Дата обязательна")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Время обязательно")]
        public string TimeRange { get; set; }

        [Required(ErrorMessage = "Количество человек обязательно")]
        [Range(1, 50, ErrorMessage = "Количество человек должно быть от 1 до 50")]
        public int PeopleCount { get; set; } = 1;

        public string Status { get; set; } = "Новое";

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public ICollection<ExcursionUploadedFile> Files { get; set; } = new List<ExcursionUploadedFile>();
    }
}
