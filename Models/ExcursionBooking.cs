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
        [Phone(ErrorMessage = "Некорректный телефон")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Школа обязательна")]
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "Дата обязательна")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Время обязательно")]
        public string TimeRange { get; set; }

        public string Status { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
