namespace LMPE_API.Models
{
    public class MessageReactionIn
    {
        public long MessageId { get; set; }     // Id du message concerné
        public long UserId { get; set; }        // Id de l'utilisateur qui réagit
        public string Emoji { get; set; }       // Emoji (ex: "👍", "❤️")
    }

    // -------------------------------
    // Model pour renvoyer une réaction (output)
    // -------------------------------
    public class MessageReactionOut
    {
        public long Id { get; set; }            // Id de la réaction
        public long MessageId { get; set; }     // Id du message
        public long UserId { get; set; }        // Id de l'utilisateur
        public string Emoji { get; set; }       // Emoji
        public DateTime CreatedAt { get; set; } // Date de création

        public string UserEmail { get; set; } = null!;
        public string UserPseudo { get; set; } = null!;
        public string? UserUrlImage { get; set; }
        public bool UserIsAdmin { get; set; }
    }
}
