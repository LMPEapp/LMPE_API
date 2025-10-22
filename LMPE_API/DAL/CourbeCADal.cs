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
                UserId = record["UserId"] == DBNull.Value ? null : Convert.ToInt64(record["UserId"]),
                DatePoint = DateOnly.FromDateTime(Convert.ToDateTime(record["DatePoint"])),
                Amount = Convert.ToDecimal(record["Amount"]),
                Description = record["Description"] as string,
                CreatedAt = Convert.ToDateTime(record["CreatedAt"]),

                UserEmail = record["Email"] != DBNull.Value ? Convert.ToString(record["Email"]) : null,
                UserPseudo = record["Pseudo"] != DBNull.Value ? Convert.ToString(record["Pseudo"]) : null,
                UserUrlImage = record["UrlImage"] == DBNull.Value ? null : record["UrlImage"] as string,
                UserIsAdmin = record["IsAdmin"] == DBNull.Value ? null : Convert.ToBoolean(record["IsAdmin"])

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

        public IEnumerable<CourbeCAGroupByDatePoint> GetAllGroupeByDate(DateOnly startDate, DateOnly endDate, long? idUser)
        {
            var list = new List<CourbeCAGroupByDatePoint>();
            using var conn = _db.GetConnection();
            conn.Open();

            var sql = @"
                SELECT 
                    DatePoint,
                    GROUP_CONCAT(Id) AS Ids,
                    SUM(Amount) AS TotalAmount,
                    COUNT(*) AS CountItems
                FROM CourbeCA
                WHERE DatePoint BETWEEN @StartDate AND @EndDate";

            // 🔹 Ajout du filtre UserId si défini
            if (idUser.HasValue)
                sql += " AND UserId = @UserId";

            sql += @"
                GROUP BY DatePoint
                ORDER BY DatePoint ASC";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate.ToDateTime(new TimeOnly(0, 0)));
            cmd.Parameters.AddWithValue("@EndDate", endDate.ToDateTime(new TimeOnly(0, 0)));

            if (idUser.HasValue)
                cmd.Parameters.AddWithValue("@UserId", idUser.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CourbeCAGroupByDatePoint
                {
                    ids = reader["Ids"].ToString(),
                    DatePoint = DateOnly.FromDateTime(Convert.ToDateTime(reader["DatePoint"])),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    CountItems = Convert.ToDecimal(reader["CountItems"]) // Count(*) retourne un long
                });
            }

            return list;
        }


        public IEnumerable<CourbeCA> GetAll(long? lastId, int pageSize, long? idUser)
        {
            var list = new List<CourbeCA>();
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT ca.*, u.Email, u.IsAdmin, u.Pseudo, u.UrlImage
                FROM CourbeCA ca
                LEFT JOIN Users u ON ca.UserId = u.Id
                /**WHERE_CLAUSE**/
                ORDER BY ca.Id DESC
                LIMIT @PageSize";

            var whereClauses = new List<string>();

            if (lastId.HasValue)
                whereClauses.Add("ca.Id < @LastId");

            if (idUser.HasValue)
                whereClauses.Add("ca.UserId = @UserId");

            // Construction du WHERE dynamique
            if (whereClauses.Count > 0)
                sql = sql.Replace("/**WHERE_CLAUSE**/", "WHERE " + string.Join(" AND ", whereClauses));
            else
                sql = sql.Replace("/**WHERE_CLAUSE**/", "");

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            if (lastId.HasValue)
                cmd.Parameters.AddWithValue("@LastId", lastId.Value);

            if (idUser.HasValue)
                cmd.Parameters.AddWithValue("@UserId", idUser.Value);

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
                LEFT JOIN Users u ON ca.UserId = u.Id
                WHERE ca.Id=@Id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            using var reader = cmd.ExecuteReader();
            Console.WriteLine(reader.ToString());
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

        public decimal GetSum(DateOnly startDate, DateOnly endDate)
        {
            using var conn = _db.GetConnection();
            conn.Open();
            var sql = @"SELECT COALESCE(SUM(Amount),0) FROM CourbeCA 
                    WHERE DatePoint BETWEEN @StartDate AND @EndDate";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate.ToDateTime(new TimeOnly(0, 0)));
            cmd.Parameters.AddWithValue("@EndDate", endDate.ToDateTime(new TimeOnly(0, 0)));
            return Convert.ToDecimal(cmd.ExecuteScalar());
        }
    }
}
