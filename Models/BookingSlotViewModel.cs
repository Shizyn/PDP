namespace DP.Models
{
    public class BookingSlotViewModel
    {
        public DateTime SelectedDate { get; set; }
        public int SelectedHour { get; set; }
        public IEnumerable<DateTime> AvailableDates { get; set; }
        public IEnumerable<int> AvailableHours { get; set; }
        public string BookingType { get; set; } // "ProfProba" or "Excursion"
    }
}
