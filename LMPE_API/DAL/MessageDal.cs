using LMPE_API.Data;
using LMPE_API.Models;
using MySqlConnector;
using System.Data;

namespace LMPE_API.DAL
{
    public static class MessageMapper
    {
        public static MessageOut Map(IDataRecord record) => new MessageOut
        {
            Id = Convert.ToInt64(record["Id"]),
            GroupeId = Convert.ToInt64(record["GroupeId"]),
            UserId = Convert.ToInt64(record["UserId"]),
            Type = record["Type"].ToString()!,
            Content = record["Content"].ToString()!,
            CreatedAt = Convert.ToDateTime(record["CreatedAt"]),

            UserEmail = record["Email"].ToString()!,
            UserPseudo = record["Pseudo"].ToString()!,
            UserUrlImage = record["UrlImage"] == DBNull.Value ? null : record["UrlImage"].ToString(),
            UserIsAdmin = Convert.ToBoolean(record["IsAdmin"]),

            IsRead = Convert.ToInt32(record["IsRead"]) > 0
        };
    }


    public class MessageDal
    {
        private readonly Database _db;

        public MessageDal(Database db)
        {
            _db = db;
        }

        // -------------------- Messages par groupe --------------------
        public IEnumerable<MessageOut> GetByGroupId(long groupId, long userId, int limit, long? lastMessageId = null)
        {
            var messages = new Dictionary<long, MessageOut>();

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT 
                    m.*, 
                    u.Email, u.Pseudo, u.UrlImage, u.IsAdmin,
                    (n.MessageId IS NOT NULL) AS IsRead,
                    r.Id AS ReactionId, r.UserId AS ReactionUserId, r.Emoji AS ReactionEmoji, r.CreatedAt AS ReactionCreatedAt,
                    ru.Email AS ReactionUserEmail, ru.Pseudo AS ReactionUserPseudo, ru.UrlImage AS ReactionUserUrlImage, ru.IsAdmin AS ReactionUserIsAdmin
                FROM Message m
                JOIN Users u ON u.Id = m.UserId
                LEFT JOIN Notification_User_Message n 
                    ON m.Id = n.MessageId AND n.UserId = @UserId
                LEFT JOIN Message_Reaction r 
                    ON m.Id = r.MessageId
                LEFT JOIN Users ru ON r.UserId = ru.Id
                WHERE m.GroupeId=@GroupeId" + (lastMessageId != null ? " AND m.Id < @LastId" : "") + @"
                ORDER BY m.Id DESC
                LIMIT @Limit;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@GroupeId", groupId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Limit", limit);
            if (lastMessageId != null)
                cmd.Parameters.AddWithValue("@LastId", lastMessageId.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var messageId = Convert.ToInt64(reader["Id"]);

                if (!messages.TryGetValue(messageId, out var message))
                {
                    message = MessageMapper.Map(reader);
                    messages[messageId] = message;
                }

                // Si la ligne contient une réaction
                if (reader["ReactionId"] != DBNull.Value)
                {
                    var reaction = new MessageReactionOut
                    {
                        Id = Convert.ToInt64(reader["ReactionId"]),
                        MessageId = messageId,
                        UserId = Convert.ToInt64(reader["ReactionUserId"]),
                        Emoji = reader["ReactionEmoji"].ToString()!,
                        CreatedAt = Convert.ToDateTime(reader["ReactionCreatedAt"]),

                        UserEmail = reader["ReactionUserEmail"].ToString()!,
                        UserPseudo = reader["ReactionUserPseudo"].ToString()!,
                        UserUrlImage = reader["ReactionUserUrlImage"] == DBNull.Value ? null : reader["ReactionUserUrlImage"].ToString(),
                        UserIsAdmin = Convert.ToBoolean(reader["ReactionUserIsAdmin"])
                    };
                    message.Reactions.Add(reaction);
                }
            }

            return messages.Values.OrderBy(m => m.Id);
        }




        public MessageOut? GetById(long messageId, long userId)
        {
            MessageOut? message = null;

            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT 
                    m.*, 
                    u.Email, u.Pseudo, u.UrlImage, u.IsAdmin,
                    (n.MessageId IS NOT NULL) AS IsRead,
                    r.Id AS ReactionId, r.UserId AS ReactionUserId, r.Emoji AS ReactionEmoji, r.CreatedAt AS ReactionCreatedAt,
                    ru.Email AS ReactionUserEmail, ru.Pseudo AS ReactionUserPseudo, ru.UrlImage AS ReactionUserUrlImage, ru.IsAdmin AS ReactionUserIsAdmin
                FROM Message m
                JOIN Users u ON u.Id = m.UserId
                LEFT JOIN Notification_User_Message n 
                    ON m.Id = n.MessageId AND n.UserId = @UserId
                LEFT JOIN Message_Reaction r
                    ON m.Id = r.MessageId
                LEFT JOIN Users ru ON r.UserId = ru.Id
                WHERE m.Id = @Id;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", messageId);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (message == null)
                {
                    message = MessageMapper.Map(reader);
                }

                // Si la ligne contient une réaction
                if (reader["ReactionId"] != DBNull.Value)
                {
                    var reaction = new MessageReactionOut
                    {
                        Id = Convert.ToInt64(reader["ReactionId"]),
                        MessageId = messageId,
                        UserId = Convert.ToInt64(reader["ReactionUserId"]),
                        Emoji = reader["ReactionEmoji"].ToString()!,
                        CreatedAt = Convert.ToDateTime(reader["ReactionCreatedAt"]),

                        UserEmail = reader["ReactionUserEmail"].ToString()!,
                        UserPseudo = reader["ReactionUserPseudo"].ToString()!,
                        UserUrlImage = reader["ReactionUserUrlImage"] == DBNull.Value ? null : reader["ReactionUserUrlImage"].ToString(),
                        UserIsAdmin = Convert.ToBoolean(reader["ReactionUserIsAdmin"])
                    };
                    message.Reactions.Add(reaction);
                }
            }

            return message;
        }





        public long Insert(long groupId, MessageIn m)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // 1️ Insertion du message
                using var cmd = new MySqlCommand(@"
                    INSERT INTO Message (GroupeId, UserId, Type, Content)
                    VALUES (@GroupeId, @UserId, @Type, @Content);
                    SELECT LAST_INSERT_ID();", conn, tran);

                cmd.Parameters.AddWithValue("@GroupeId", groupId);
                cmd.Parameters.AddWithValue("@UserId", m.UserId);
                cmd.Parameters.AddWithValue("@Type", m.Type);
                cmd.Parameters.AddWithValue("@Content", m.Content);

                var messageId = Convert.ToInt64(cmd.ExecuteScalar());

                // 2️ Création des notifications pour tous les autres membres du groupe
                using var notifCmd = new MySqlCommand(@"
                    INSERT INTO Notification_User_Message (UserId, MessageId)
                    SELECT ug.UserId, @MessageId
                    FROM User_Groupe ug
                    WHERE ug.GroupeId = @GroupeId
                        AND ug.UserId <> @AuthorId;", conn, tran);

                notifCmd.Parameters.AddWithValue("@MessageId", messageId);
                notifCmd.Parameters.AddWithValue("@GroupeId", groupId);
                notifCmd.Parameters.AddWithValue("@AuthorId", m.UserId);

                notifCmd.ExecuteNonQuery();

                tran.Commit();
                return messageId;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        public int ReadAllMessagesForUser(long groupId, long userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                // Suppression de toutes les notifications non lues pour ce groupe et cet utilisateur
                using var cmd = new MySqlCommand(@"
                    DELETE NUM
                    FROM Notification_User_Message NUM
                    INNER JOIN Message M ON NUM.MessageId = M.Id
                    WHERE NUM.UserId = @UserId
                        AND M.GroupeId = @GroupeId;", conn, tran);

                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@GroupeId", groupId);

                int rowsDeleted = cmd.ExecuteNonQuery();

                tran.Commit();
                return rowsDeleted;
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public IEnumerable<long> GetUserIdsToNotify(long groupId)
        {
            var userIds = new List<long>();

            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(@"
                SELECT ug.UserId
                FROM User_Groupe ug
                WHERE ug.GroupeId = @GroupeId;", conn);

            cmd.Parameters.AddWithValue("@GroupeId", groupId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                userIds.Add(reader.GetInt64(0));
            }

            return userIds;
        }



        public int GetTotalUnreadNotifications(long userId)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            var sql = @"
                SELECT COUNT(*) AS TotalCount 
                FROM Notification_User_Message 
                WHERE UserId = @UserId;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result);
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
        public IEnumerable<Message> GetOldMessages(int days = 30)
        {
            var list = new List<Message>();
            using var conn = _db.GetConnection();
            conn.Open();

            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            string sql = @"
                SELECT Id, GroupeId, UserId, Type, Content, CreatedAt
                FROM Message
                WHERE CreatedAt < @CutoffDate
                ORDER BY Id ASC;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@CutoffDate", cutoffDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Message
                {
                    Id = reader.GetInt64("Id"),
                    GroupeId = reader.GetInt64("GroupeId"),
                    UserId = reader.GetInt64("UserId"),
                    Type = reader["Type"].ToString()!,
                    Content = reader["Content"].ToString()!,
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return list;
        }


    }
}
