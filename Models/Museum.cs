namespace DP.Models
{
    public class Museum
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ExcursionBooking> Excursions { get; set; } = new List<ExcursionBooking>();
    }
}
