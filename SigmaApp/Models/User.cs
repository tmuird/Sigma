using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SigmaApp.Models
{
    public class User
    {
        [Key]
        public string UserID { get; set; } = null!;
        public string? PublicKey { get; set; }
    }
}
