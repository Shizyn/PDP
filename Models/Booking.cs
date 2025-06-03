using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static DP.Controllers.BookingController;

namespace DP.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        [ForeignKey("ProfProba")]
        [Column("TrialID")]
        public int ProfProbaId { get; set; }
        public ProfProba ProfProba { get; set; }
        
        [Required(ErrorMessage = "ФИО обязательно для заполнения")]
        [RegularExpression(@"^[a-zA-Zа-яА-Я\s]+$", ErrorMessage = "ФИО может содержать только буквы и пробелы")]
        public string FullName { get; set; }
        //[DataType(DataType.Date)]
        //[Display(Name = "Дата рождения")]
        //public DateTime BirthDate { get; set; }
        [Required(ErrorMessage = "Телефон обязателен для заполнения")]
        [RegularExpression(@"^8\(\d{3}\)-\d{3}-\d{2}-\d{2}$", ErrorMessage = "Телефон должен быть в формате 8(XXX)-XXX-XX-XX")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Некорректный формат электронной почты")]
        public string Email { get; set; }

        public string SchoolName { get; set; }
        public string TypeEvent { get; set; }
        [Required]
        public string TimeRange { get; set; }
        public int PeopleCount { get; set; }
        [Required]
        public DateTime BookingDate { get; set; }
        [Required]
        public string Status { get; set; }
        public virtual ICollection<UploadedFile> Files { get; set; }
    }
}
