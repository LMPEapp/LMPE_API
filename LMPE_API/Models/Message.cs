using Microsoft.AspNetCore.Mvc;

namespace LMPE_API.Models
{
    public class Message
    {
        public long Id { get; set; }
        public long GroupeId { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = "texte"; // 'texte','image','video','fichier'
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MessageIn
    {
        public long UserId { get; set; } // peut être récupéré depuis JWT
        public string Type { get; set; } = "texte";
        public string Content { get; set; } = "";
    }

}
