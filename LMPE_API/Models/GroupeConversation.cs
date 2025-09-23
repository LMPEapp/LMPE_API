namespace LMPE_API.Models
{
    public class GroupeConversation
    {
        public long Id { get; set; }
        public string Nom { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class GroupeConversationIn
    {
        public string Nom { get; set; } = "";
    }
    public class UserGroupeIn
    {
        public List<long> UserIds { get; set; } = new();
    }
}
