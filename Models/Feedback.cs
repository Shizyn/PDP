using System.ComponentModel.DataAnnotations;

namespace DP.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? BookingId { get; set; }
        public virtual Booking Booking { get; set; }

        public int? ExcursionBookingId { get; set; }
        public virtual ExcursionBooking ExcursionBooking { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
