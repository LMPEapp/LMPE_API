using Microsoft.AspNetCore.Mvc;

namespace LMPE_API.Models
{
    public class MessageOut
    {
        public long Id { get; set; }
        public long GroupeId { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = "texte";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }

        // Infos utilisateur
        public string UserEmail { get; set; } = "";
        public string UserPseudo { get; set; } = "";
        public string? UserUrlImage { get; set; }
        public bool UserIsAdmin { get; set; }
    }
    public class MessageIn
    {
        public long UserId { get; set; } // peut être récupéré depuis JWT
        public string Type { get; set; } = "texte";
        public string Content { get; set; } = "";
    }

}
