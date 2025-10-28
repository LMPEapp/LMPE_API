using LMPE_API.Data;
using MySqlConnector;
using System;
using static LMPE_API.Models.AuthModels;

namespace TonNamespace.DAL
{
    public class RefreshTokenDAL
    {
        private readonly Database _db;

        public RefreshTokenDAL(Database db)
        {
            _db = db;
        }

        // 🔍 Récupère l'ID utilisateur à partir d'un refresh token
        public long? GetUserIdByRefreshToken(string refreshToken)
        {
            DeleteOldTokens();

            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                "SELECT UserId FROM RefreshTokens WHERE RefreshToken = @RefreshToken LIMIT 1",
                conn
            );
            cmd.Parameters.AddWithValue("@RefreshToken", refreshToken);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return Convert.ToInt64(reader["UserId"]);

            return null;
        }

        // ➕ Insère un nouveau refresh token
        public long Insert(RefreshTokenIN tokenIn, long userId)
        {
            DeleteOldTokens();

            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(@"
                INSERT INTO RefreshTokens (UserId, RefreshToken)
                VALUES (@UserId, @RefreshToken);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@RefreshToken", tokenIn.RefreshToken);

            return Convert.ToInt64(cmd.ExecuteScalar());
        }



        private void DeleteOldTokens()
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(
                "DELETE FROM RefreshTokens WHERE CreatedAt < NOW() - INTERVAL 30 DAY;",
                conn
            );

            cmd.ExecuteNonQuery();
        }
    }
}
