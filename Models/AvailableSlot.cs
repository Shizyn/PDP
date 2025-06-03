namespace DP.Models
{
    public class AvailableSlot
    {
        public int Id { get; set; }
        public int ProfProbaId { get; set; }
        public DateTime SlotDate { get; set; }
        public string TimeRange { get; set; }
    }
}
