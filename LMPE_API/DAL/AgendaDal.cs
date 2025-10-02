using LMPE_API.Data;
using LMPE_API.Models;
using MySqlConnector;
using System.Data;

namespace LMPE_API.DAL
{
    public static class AgendaMapper
    {
        public static AgendaOut Map(IDataRecord record) => new AgendaOut
        {
            Id = Convert.ToInt64(record["Id"]),
            Title = record["Title"].ToString()!,
            Description = record["Description"] == DBNull.Value ? null : record["Description"].ToString(),
            StartDate = Convert.ToDateTime(record["StartDate"]),
            EndDate = Convert.ToDateTime(record["EndDate"]),
            IsPublic = Convert.ToBoolean(record["IsPublic"]),
            CreatedAt = Convert.ToDateTime(record["CreatedAt"]),
            CreatedBy = record["CreatedBy"] == DBNull.Value ? null : Convert.ToInt64(record["CreatedBy"]),
            CreatorEmail = record["CreatorEmail"] == DBNull.Value ? null : record["CreatorEmail"].ToString(),
            CreatorPseudo = record["CreatorPseudo"] == DBNull.Value ? null : record["CreatorPseudo"].ToString(),
            CreatorUrlImage = record["CreatorUrlImage"] == DBNull.Value ? null : record["CreatorUrlImage"].ToString(),
            CreatorIsAdmin = record["CreatorIsAdmin"] == DBNull.Value ? false : Convert.ToBoolean(record["CreatorIsAdmin"])
        };
    }
    public class AgendaDal
    {
        private readonly Database _db;

        public AgendaDal(Database db)
        {
            _db = db;
        }

        public IEnumerable<AgendaOut> GetAll(DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!startDate.HasValue || !endDate.HasValue)
            {
                throw new ArgumentException("Les paramètres startDate et endDate sont obligatoires.");
            }

            var interval = endDate.Value - startDate.Value;
            if (interval.TotalDays > 31)
            {
                throw new ArgumentException("La période entre startDate et endDate ne peut pas dépasser 1 mois.");
            }

            var list = new List<AgendaOut>();
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT a.*, 
                       u.Email AS CreatorEmail, u.Pseudo AS CreatorPseudo, 
                       u.UrlImage AS CreatorUrlImage, u.IsAdmin AS CreatorIsAdmin
                FROM Agenda a
                LEFT JOIN Users u ON u.Id = a.CreatedBy
                WHERE a.StartDate >= @StartDate AND a.EndDate <= @EndDate";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
            cmd.Parameters.AddWithValue("@EndDate", endDate.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(AgendaMapper.Map(reader));
            }

            return list;
        }


        public AgendaOut? GetById(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            string sql = @"
                SELECT a.*, 
                       u.Email AS CreatorEmail, u.Pseudo AS CreatorPseudo, 
                       u.UrlImage AS CreatorUrlImage, u.IsAdmin AS CreatorIsAdmin
                FROM Agenda a
                LEFT JOIN Users u ON u.Id = a.CreatedBy
                WHERE a.Id = @Id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return AgendaMapper.Map(reader);
            }

            return null;
        }

        public long Insert(AgendaIn input)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(@"
                INSERT INTO Agenda (Title, Description, StartDate, EndDate, CreatedBy, IsPublic)
                VALUES (@Title, @Description, @StartDate, @EndDate, @CreatedBy, @IsPublic);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@Title", input.Title);
            cmd.Parameters.AddWithValue("@Description", (object?)input.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", input.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", input.EndDate);
            cmd.Parameters.AddWithValue("@CreatedBy", input.CreatedBy);
            cmd.Parameters.AddWithValue("@IsPublic", input.IsPublic);

            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        public bool Update(long id, AgendaIn input)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand(@"
                UPDATE Agenda
                SET Title=@Title, Description=@Description, StartDate=@StartDate, EndDate=@EndDate, IsPublic=@IsPublic
                WHERE Id=@Id", conn);

            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Title", input.Title);
            cmd.Parameters.AddWithValue("@Description", (object?)input.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", input.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", input.EndDate);
            cmd.Parameters.AddWithValue("@IsPublic", input.IsPublic);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(long id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var cmd = new MySqlCommand("DELETE FROM Agenda WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }


        public void AddUsersToAgenda(long agendaId, List<long> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return;

            using var conn = _db.GetConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();
            foreach (var userId in userIds)
            {
                var cmd = conn.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText = @"
                    INSERT IGNORE INTO Agenda_User (AgendaId, UserId)
                    VALUES (@AgendaId, @UserId)";
                cmd.Parameters.AddWithValue("@AgendaId", agendaId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
        }

        public void RemoveUsersFromAgenda(long agendaId, List<long> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return;

            using var conn = _db.GetConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();

            foreach (var userId in userIds)
            {
                var cmd = conn.CreateCommand();
                cmd.Transaction = tran;
                cmd.CommandText = @"
                    DELETE FROM Agenda_User
                    WHERE AgendaId = @AgendaId AND UserId = @UserId";
                cmd.Parameters.AddWithValue("@AgendaId", agendaId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }

            tran.Commit();
        }


        public IEnumerable<User> GetUsersForAgenda(long agendaId)
        {
            var list = new List<User>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT u.*
                FROM Users u
                INNER JOIN Agenda_User au ON au.UserId = u.Id
                WHERE au.AgendaId = @AgendaId";
            cmd.Parameters.AddWithValue("@AgendaId", agendaId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(UserMapper.Map(reader));
            }

            return list;
        }
    }
}
