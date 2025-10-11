using LMPE_API.Data;
using LMPE_API.Models;
using MySqlConnector;
using System.Data;

namespace LMPE_API.DAL
{
    public static class MessageReactionMapper
    {
        public static MessageReactionOut Map(IDataRecord record)
        {
            return new MessageReactionOut
            {
                Id = Convert.ToInt64(record["Id"]),
                MessageId = Convert.ToInt64(record["MessageId"]),
                UserId = Convert.ToInt64(record["UserId"]),
                Emoji = record["Emoji"].ToString()!,
                CreatedAt = Convert.ToDateTime(record["CreatedAt"]),
                UserEmail = record["UserEmail"].ToString()!,
                UserPseudo = record["UserPseudo"].ToString()!,
                UserUrlImage = record["UserUrlImage"] as string,
                UserIsAdmin = Convert.ToBoolean(record["UserIsAdmin"])
            };
        }
    }

    public class MessageReactionDal
    {
        private readonly Database _db;

        public MessageReactionDal(Database db)
        {
            _db = db;
        }

        // -------------------------------
        // Insert une réaction
        // -------------------------------
        public long Insert(MessageReactionIn reaction)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var sql = @"
                INSERT INTO Message_Reaction (MessageId, UserId, Emoji)
                VALUES (@MessageId, @UserId, @Emoji)
                ON DUPLICATE KEY UPDATE
                    Emoji = VALUES(Emoji),
                    CreatedAt = CURRENT_TIMESTAMP;
                SELECT Id FROM Message_Reaction WHERE MessageId = @MessageId AND UserId = @UserId;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MessageId", reaction.MessageId);
            cmd.Parameters.AddWithValue("@UserId", reaction.UserId);
            cmd.Parameters.AddWithValue("@Emoji", reaction.Emoji);

            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        public MessageReactionOut? GetById(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var sql = @"
                SELECT 
                    mr.Id,
                    mr.MessageId,
                    mr.UserId,
                    mr.Emoji,
                    mr.CreatedAt,
                    u.Email AS UserEmail,
                    u.Pseudo AS UserPseudo,
                    u.UrlImage AS UserUrlImage,
                    u.IsAdmin AS UserIsAdmin
                FROM Message_Reaction mr
                JOIN Users u ON u.Id = mr.UserId
                WHERE mr.Id = @Id;
            ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return MessageReactionMapper.Map(reader);

            return null;
        }


        // -------------------------------
        // Supprime une réaction (par Id)
        // -------------------------------
        public bool Delete(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("DELETE FROM Message_Reaction WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
