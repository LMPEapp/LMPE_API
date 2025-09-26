namespace LMPE_API.Models
{
    public class GroupeConversation
    {
        public long Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime LastActivity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class GroupeConversationIn
    {
        public string Name { get; set; } = "";
    }
    public class UserGroupeIn
    {
        public List<long> UserIds { get; set; } = new();
    }
}
