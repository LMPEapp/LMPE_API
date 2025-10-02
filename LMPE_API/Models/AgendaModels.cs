using Microsoft.AspNetCore.Mvc;

namespace LMPE_API.Models
{
    public class AgendaOut
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }

        // Infos créateur
        public long? CreatedBy { get; set; }
        public string? CreatorEmail { get; set; }
        public string? CreatorPseudo { get; set; }
        public string? CreatorUrlImage { get; set; }
        public bool CreatorIsAdmin { get; set; }
    }

    public class AgendaIn
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long? CreatedBy { get; set; }
        public bool IsPublic { get; set; } = false;
    }
    public class AgendaUserIn
    {
        public List<long> UserIds { get; set; } = new();
    }

}
