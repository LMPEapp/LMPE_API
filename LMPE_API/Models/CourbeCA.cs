namespace LMPE_API.Models
{
    public class CourbeCA
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public DateOnly DatePoint { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? UserEmail { get; set; } = null!;
        public string? UserPseudo { get; set; } = null!;
        public string? UserUrlImage { get; set; }
        public bool? UserIsAdmin { get; set; }
    }

    public class CourbeCAGroupByDatePoint
    {
        public string ids { get; set; }
        public DateOnly DatePoint { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CountItems { get; set; }
    }

    public class CourbeCAIn
    {
        public long? UserId { get; set; }
        public DateOnly DatePoint { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

}
