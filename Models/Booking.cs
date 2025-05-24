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

        [ForeignKey("Event")]
        public int EventId { get; set; }

        public Event Event { get; set; }
        [Required]
        public string FullName { get; set; }
        //[DataType(DataType.Date)]
        //[Display(Name = "Дата рождения")]
        //public DateTime BirthDate { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string SchoolName { get; set; }
        public string TypeEvent { get; set; }
        [Required]
        public string TimeRange { get; set; }
        [Required]
        public DateTime BookingDate { get; set; }
        [Required]
        public string Status { get; set; }
        public virtual ICollection<UploadedFile> Files { get; set; }
    }
}
