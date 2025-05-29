using System.ComponentModel.DataAnnotations;

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
    }
}
