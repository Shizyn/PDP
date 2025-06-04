namespace DP.Models
{
    public class ExcursionUploadedFile
    {
        public int Id { get; set; }
        public int ExcursionBookingId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] Content { get; set; }

        public ExcursionBooking ExcursionBooking { get; set; }
    }
}
