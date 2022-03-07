

namespace SigmaApp.Models
{
    public class Message
    {
        public int? Id { get; set; }
        public User? Sender { get; set; }
        public User? Receiver { get; set; }
        public string? Content { get; set; }
        public bool? IsMine { get; set; }
        public string? HMAC { get; set; }
        public string? InitVector { get; set; }
        public DateTime Creation { get; set; }



    }
}
