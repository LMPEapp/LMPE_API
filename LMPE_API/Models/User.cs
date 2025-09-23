namespace LMPE_API.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public string Pseudo { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? UrlImage { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserIn
    {
        public string Email { get; set; } = null!;
        public string Pseudo { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? UrlImage { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}
