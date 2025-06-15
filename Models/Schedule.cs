namespace DP.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public int? ProfProbaId { get; set; }
        public int? ExcursionId { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public virtual ProfProba ProfProba { get; set; }
        public virtual ExcursionBooking Excursion { get; set; }
    }
}
