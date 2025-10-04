namespace LMPE_API.Models
{
    public class CourbeCA
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime DatePoint { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserEmail { get; set; } = null!;
        public string UserPseudo { get; set; } = null!;
        public string? UserUrlImage { get; set; }
        public bool UserIsAdmin { get; set; }
    }

    public class CourbeCAIn
    {
        public long UserId { get; set; }
        public DateTime DatePoint { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

}
