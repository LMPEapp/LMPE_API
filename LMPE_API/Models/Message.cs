using Microsoft.AspNetCore.Mvc;

namespace LMPE_API.Models
{
    public class Message
    {
        public long Id { get; set; }
        public long GroupeId { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = "texte";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
    public class MessageOut
    {
        public long Id { get; set; }
        public long GroupeId { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = "texte";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        
        public long? ParentId { get; set; }

        // Infos utilisateur
        public string UserEmail { get; set; } = "";
        public string UserPseudo { get; set; } = "";
        public string? UserUrlImage { get; set; }
        public bool UserIsAdmin { get; set; }

        public bool IsRead { get; set; } = false;

        public List<MessageReactionOut> Reactions { get; set; } = new List<MessageReactionOut>();
        
        public long? ParentGroupeId { get; set; }
        public long? ParentUserId { get; set; }
        public string? ParentType { get; set; } = "texte";
        public string? ParentContent { get; set; } = "";
        public DateTime? ParentCreatedAt { get; set; }

        // Infos utilisateur
        public string? ParentUserEmail { get; set; } = "";
        public string? ParentUserPseudo { get; set; } = "";
        public string? ParentUserUrlImage { get; set; }
        public bool? ParentUserIsAdmin { get; set; }
    }
    public class MessageIn
    {
        public long UserId { get; set; } // peut être récupéré depuis JWT
        public string Type { get; set; } = "texte";
        public string Content { get; set; } = "";
        
        public long? ParentId { get; set; }
    }

}
