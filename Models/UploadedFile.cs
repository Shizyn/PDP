using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DP.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; } // Ссылка на заявку

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FileType { get; set; } // "Excel" или "PDF"

        [Required]
        public byte[] Content { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking Booking { get; set; }
    }
}
