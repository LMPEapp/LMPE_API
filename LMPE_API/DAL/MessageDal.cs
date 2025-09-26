using LMPE_API.Data;
using LMPE_API.Models;
using MySqlConnector;
using System.Data;

namespace LMPE_API.DAL
{
    public static class MessageMapper
    {
        public static Message Map(IDataRecord record)
        {
            return new Message
            {
                Id = Convert.ToInt64(record["Id"]),
                GroupeId = Convert.ToInt64(record["GroupeId"]),
                UserId = Convert.ToInt64(record["UserId"]),
                Type = Convert.ToString(record["Type"])!,
                Content = Convert.ToString(record["Content"])!,
                CreatedAt = Convert.ToDateTime(record["CreatedAt"])
            };
        }
    }

    public class MessageDal
    {
        private readonly Database _db;

        public MessageDal(Database db)
        {
            _db = db;
        }

        // -------------------- Messages par groupe --------------------
        public IEnumerable<MessageOut> GetByGroupId(long groupId, int limit, long? lastMessageId = null)
        {
            var list = new List<MessageOut>();
            using var conn = _db.GetConnection();
            conn.Open();

            string sql;
            if (lastMessageId == null)
            {
                sql = @"
            SELECT m.*, u.Email, u.Pseudo, u.UrlImage, u.IsAdmin
            FROM Message m
            JOIN Users u ON u.Id = m.UserId
            WHERE m.GroupeId=@GroupeId
            ORDER BY m.Id DESC
            LIMIT @Limit";
            }
            else
            {
                sql = @"
            SELECT m.*, u.Email, u.Pseudo, u.UrlImage, u.IsAdmin
            FROM Message m
            JOIN Users u ON u.Id = m.UserId
            WHERE m.GroupeId=@GroupeId AND m.Id < @LastId
            ORDER BY m.Id DESC
            LIMIT @Limit";
            }

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@GroupeId", groupId);
            cmd.Parameters.AddWithValue("@Limit", limit);
            if (lastMessageId != null)
                cmd.Parameters.AddWithValue("@LastId", lastMessageId.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new MessageOut
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    GroupeId = Convert.ToInt64(reader["GroupeId"]),
                    UserId = Convert.ToInt64(reader["UserId"]),
                    Type = reader["Type"].ToString()!,
                    Content = reader["Content"].ToString()!,
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),

                    UserEmail = reader["Email"].ToString()!,
                    UserPseudo = reader["Pseudo"].ToString()!,
                    UserUrlImage = reader["UrlImage"] == DBNull.Value ? null : reader["UrlImage"].ToString(),
                    UserIsAdmin = Convert.ToBoolean(reader["IsAdmin"])
                });
            }

            return list.OrderBy(m => m.Id);
        }

        public MessageOut? GetById(long Id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
        SELECT m.*, u.Email, u.Pseudo, u.UrlImage, u.IsAdmin
        FROM Message m
        JOIN Users u ON u.Id = m.UserId
        WHERE m.Id=@Id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", Id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new MessageOut
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    GroupeId = Convert.ToInt64(reader["GroupeId"]),
                    UserId = Convert.ToInt64(reader["UserId"]),
                    Type = reader["Type"].ToString()!,
                    Content = reader["Content"].ToString()!,
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),

                    UserEmail = reader["Email"].ToString()!,
                    UserPseudo = reader["Pseudo"].ToString()!,
                    UserUrlImage = reader["UrlImage"] == DBNull.Value ? null : reader["UrlImage"].ToString(),
                    UserIsAdmin = Convert.ToBoolean(reader["IsAdmin"])
                };
            }

            return null;
        }



        public long Insert(long groupId, MessageIn m)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                INSERT INTO Message (GroupeId, UserId, Type, Content)
                VALUES (@GroupeId, @UserId, @Type, @Content);
                SELECT LAST_INSERT_ID();", conn);
            cmd.Parameters.AddWithValue("@GroupeId", groupId);
            cmd.Parameters.AddWithValue("@UserId", m.UserId);
            cmd.Parameters.AddWithValue("@Type", m.Type);
            cmd.Parameters.AddWithValue("@Content", m.Content);
            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        public bool Update(long messageId, MessageIn m)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                UPDATE Message
                SET Type=@Type, Content=@Content
                WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", messageId);
            cmd.Parameters.AddWithValue("@Type", m.Type);
            cmd.Parameters.AddWithValue("@Content", m.Content);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(long messageId)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM Message WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", messageId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
