using LMPE_API.Data;
using LMPE_API.Models;
using MySqlConnector;
using System.Data;

namespace LMPE_API.DAL
{
    public static class CourbeCAMapper
    {
        public static CourbeCA Map(IDataRecord record)
        {
            return new CourbeCA
            {
                Id = Convert.ToInt64(record["Id"]),
                UserId = Convert.ToInt64(record["UserId"]),
                DatePoint = Convert.ToDateTime(record["DatePoint"]),
                Amount = Convert.ToDecimal(record["Amount"]),
                Description = record["Description"] as string,
                CreatedAt = Convert.ToDateTime(record["CreatedAt"]),

                UserEmail = Convert.ToString(record["Email"])!,
                UserPseudo = Convert.ToString(record["Pseudo"])!,
                UserUrlImage = record["UrlImage"] as string,
                UserIsAdmin = Convert.ToBoolean(record["IsAdmin"])
            };
        }
    }

    public class CourbeCADal
    {
        private readonly Database _db;

        public CourbeCADal(Database db)
        {
            _db = db;
        }

        public IEnumerable<CourbeCA> GetAll(DateTime startDate, DateTime endDate)
        {
            var list = new List<CourbeCA>();
            using var conn = _db.GetConnection();
            conn.Open();

            var sql = @"
                SELECT ca.*, u.Email, u.Pseudo, u.UrlImage, u.IsAdmin
                FROM CourbeCA ca
                INNER JOIN Users u ON ca.UserId = u.Id
                WHERE ca.DatePoint BETWEEN @StartDate AND @EndDate
                ORDER BY ca.DatePoint ASC";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(CourbeCAMapper.Map(reader));
            }
            return list;
        }


        public CourbeCA? GetById(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var sql = @"
                SELECT ca.*, u.Email, u.Pseudo, u.UrlImage, u.IsAdmin
                FROM CourbeCA ca
                INNER JOIN Users u ON ca.UserId = u.Id
                WHERE ca.Id=@Id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? CourbeCAMapper.Map(reader) : null;
        }

        public long Insert(CourbeCAIn ca)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var sql = @"
                INSERT INTO CourbeCA (UserId, DatePoint, Amount, Description) 
                VALUES (@UserId, @DatePoint, @Amount, @Description);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", ca.UserId);
            cmd.Parameters.AddWithValue("@DatePoint", ca.DatePoint);
            cmd.Parameters.AddWithValue("@Amount", ca.Amount);
            cmd.Parameters.AddWithValue("@Description", ca.Description ?? (object)DBNull.Value);

            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        public bool Delete(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("DELETE FROM CourbeCA WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public decimal GetSum(DateTime startDate, DateTime endDate)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var sql = @"SELECT COALESCE(SUM(Amount),0) FROM CourbeCA 
                    WHERE DatePoint BETWEEN @StartDate AND @EndDate";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }
    }
}
