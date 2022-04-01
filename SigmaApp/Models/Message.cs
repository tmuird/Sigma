

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigmaApp.Models
{

    public class Message
    {
        [Key]
        public int? MessageID { get; set; }
        [NotMapped]
        public User? Sender { get; set; }
        [NotMapped]
        public User? Receiver { get; set; }
        
        public string? Content { get; set; }
        
        public bool? IsMine { get; set; }
        [NotMapped]
        public string? HMAC { get; set; }
        [NotMapped]
        public string? InitVector { get; set; }
        public DateTime Creation { get; set; }
        public int? ConversationID {get; set;}

        public Conversation? Conversation { get; set; }
      
 


    }
}
