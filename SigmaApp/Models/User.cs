namespace SigmaApp.Models
{
    public class User
    {
        public string UserId { get; set; } = null!;
        public string? PublicKey { get; set; }
        public string? Creation { get; set; }
    }
}
