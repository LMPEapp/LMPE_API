using LMPE_API.Data;
using LMPE_API.Models;
using MySqlConnector;
using System.Data;

namespace LMPE_API.DAL
{
    public static class PushMapper
    {
        public static PushSubscription Map(IDataRecord record)
        {
            return new PushSubscription
            {
                Id = Convert.ToInt64(record["Id"]),
                UserId = Convert.ToInt64(record["UserId"]),
                Endpoint = Convert.ToString(record["Endpoint"])!,
                P256dh = Convert.ToString(record["P256dh"])!,
                Auth = Convert.ToString(record["Auth"])!,
                CreatedAt = Convert.ToDateTime(record["CreatedAt"])
            };
        }
    }

    public class PushDal
    {
        private readonly Database _db;

        public PushDal(Database db)
        {
            _db = db;
        }

        public long Insert(PushSubscription sub)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
                INSERT INTO PushSubscription (UserId, Endpoint, P256dh, Auth)
                VALUES (@UserId, @Endpoint, @P256dh, @Auth);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@UserId", sub.UserId);
            cmd.Parameters.AddWithValue("@Endpoint", sub.Endpoint);
            cmd.Parameters.AddWithValue("@P256dh", sub.P256dh);
            cmd.Parameters.AddWithValue("@Auth", sub.Auth);

            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        public IEnumerable<PushSubscription> GetByUserId(long userId)
        {
            var list = new List<PushSubscription>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM PushSubscription WHERE UserId=@UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(PushMapper.Map(reader));
            }
            return list;
        }

        public bool Delete(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM PushSubscription WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
