using LMPE_API.Data;
using LMPE_API.Models;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using System.Data;

namespace LMPE_API.DAL
{
    public static class UserMapper
    {
        public static User Map(IDataRecord record)
        {
            return new User
            {
                Id = Convert.ToInt64(record["Id"]),
                Email = Convert.ToString(record["Email"])!,
                Pseudo = Convert.ToString(record["Pseudo"])!,
                UrlImage = record["UrlImage"] as string,
                IsAdmin = Convert.ToBoolean(record["IsAdmin"]),
                CreatedAt = Convert.ToDateTime(record["CreatedAt"])
            };
        }
    }
    public class UserDal
    {
        private readonly Database _db;

        public UserDal(Database db)
        {
            _db = db;
        }

        public IEnumerable<User> GetAll()
        {
            var users = new List<User>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM Users", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(UserMapper.Map(reader));
            }
            return users;
        }

        public User? GetByEmail(string Email)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM Users WHERE Email=@Email", conn);
            cmd.Parameters.AddWithValue("@Email", Email);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? UserMapper.Map(reader) : null;
        }
        public string? GetPasswordByEmail(string Email)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT PasswordHash FROM Users WHERE Email=@Email", conn);
            cmd.Parameters.AddWithValue("@Email", Email);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader["PasswordHash"]?.ToString();
            }

            return null;
        }
        public User? GetById(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM Users WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? UserMapper.Map(reader) : null;
        }
        public string? GetPasswordById(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT PasswordHash FROM Users WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return reader["PasswordHash"]?.ToString();
            }

            return null;
        }

        public long Insert(UserIn u)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
            INSERT INTO Users (Email, Pseudo, PasswordHash, UrlImage, IsAdmin)
            VALUES (@Email, @Pseudo, @PasswordHash, @UrlImage, @IsAdmin);
            SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@Email", u.Email);
            cmd.Parameters.AddWithValue("@Pseudo", u.Pseudo);
            cmd.Parameters.AddWithValue("@PasswordHash", u.PasswordHash);
            cmd.Parameters.AddWithValue("@UrlImage", u.UrlImage ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsAdmin", u.IsAdmin);

            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        public bool Update(long id, UserIn u)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = "UPDATE Users SET Email=@Email, Pseudo=@Pseudo, IsAdmin=@IsAdmin";
            sql += " WHERE Id=@Id";

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Email", u.Email);
            cmd.Parameters.AddWithValue("@Pseudo", u.Pseudo);
            cmd.Parameters.AddWithValue("@IsAdmin", u.IsAdmin);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateUrlImage(long id, string imageUrl)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = "UPDATE Users SET UrlImage = @UrlImage WHERE Id = @Id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UrlImage", imageUrl);

            return cmd.ExecuteNonQuery() > 0;
        }



        public bool UpdatePassword(long id, string PasswordHash)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand(@"
            UPDATE Users
            SET PasswordHash=@PasswordHash
            WHERE Id=@Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@PasswordHash", PasswordHash);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM Users WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
