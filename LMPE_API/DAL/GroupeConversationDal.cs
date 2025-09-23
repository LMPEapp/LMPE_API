using LMPE_API.Data;
using LMPE_API.Models;
using MySqlConnector;
using System.Data;

namespace LMPE_API.DAL
{
    public static class GroupeConversationMapper
    {
        public static GroupeConversation Map(IDataRecord record)
        {
            return new GroupeConversation
            {
                Id = Convert.ToInt64(record["Id"]),
                Nom = Convert.ToString(record["Nom"])!,
                CreatedAt = Convert.ToDateTime(record["CreatedAt"])
            };
        }
    }

    public class GroupeConversationDal
    {
        private readonly Database _db;

        public GroupeConversationDal(Database db)
        {
            _db = db;
        }

        public IEnumerable<GroupeConversation> GetAll(long userId)
        {
            var list = new List<GroupeConversation>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT g.*
                FROM GroupeConversation g
                INNER JOIN User_Groupe ug ON g.Id = ug.GroupeId
                WHERE ug.UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(GroupeConversationMapper.Map(reader));
            return list;
        }


        public GroupeConversation? GetById(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM GroupeConversation WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? GroupeConversationMapper.Map(reader) : null;
        }

        public long Insert(GroupeConversationIn g)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                INSERT INTO GroupeConversation (Nom) VALUES (@Nom);
                SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@Nom", g.Nom);
            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        public bool Update(long id, GroupeConversationIn g)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("UPDATE GroupeConversation SET Nom=@Nom WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Nom", g.Nom);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM GroupeConversation WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        // Participants
        public void AddUsers(long groupId, List<long> userIds)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            foreach (var userId in userIds)
            {
                using var cmd = new MySqlCommand(@"
                    INSERT IGNORE INTO User_Groupe (GroupeId, UserId)
                    VALUES (@GroupeId, @UserId)", conn);
                cmd.Parameters.AddWithValue("@GroupeId", groupId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        public bool RemoveUser(long groupId, long userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM User_Groupe WHERE GroupeId=@GroupeId AND UserId=@UserId", conn);
            cmd.Parameters.AddWithValue("@GroupeId", groupId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public IEnumerable<User> GetUsers(long groupId)
        {
            var list = new List<User>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                SELECT u.* FROM Users u
                INNER JOIN User_Groupe ug ON u.Id = ug.UserId
                WHERE ug.GroupeId=@GroupeId", conn);
            cmd.Parameters.AddWithValue("@GroupeId", groupId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(UserMapper.Map(reader));
            return list;
        }
    }
}
