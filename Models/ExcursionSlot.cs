using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DP.Models
{
    public class ExcursionSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime SlotDate { get; set; }

        [Required]
        public string TimeRange { get; set; }
        [Required]
        public int MuseumId { get; set; } 
        [ForeignKey("MuseumId")]
        public Museum Museum { get; set; }

    }
}
