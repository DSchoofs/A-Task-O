using Dapper;
using Microsoft.Data.SqlClient;
using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public class EntryRepository : IEntryRepository
    {
        private readonly string _connectionString;
        public string sqlcmd;
        public string Fehlermeldung { get; set; }

        public EntryRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Atasko")!;
        }

        public List<Entry> GetAllEntries()
        {
            try
            {
                List<Entry> entries = new List<Entry>();
                sqlcmd = "SELECT EntryId, TaskId, UserId, StartTime, EndTime, IsCompleted FROM Entries";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    entries = conn.Query<Entry>(sqlcmd).ToList();
                    return entries;
                }
            }
            catch (SqlException sqlex)
            {
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
            }
            return new List<Entry>();
        }

        public Entry? GetEntryById(int id)
        {
            try
            {
                Entry? entry = null;
                sqlcmd = "SELECT EntryId, TaskId, UserId, StartTime, EndTime, IsCompleted FROM Entries WHERE EntryId = @Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    entry = conn.QuerySingleOrDefault<Entry>(sqlcmd, new { Id = id });
                    return entry;
                }
            }
            catch (SqlException sqlex)
            {
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
            }
            return null;
        }

        public int AddEntry(Entry entry)
        {
            try
            {
                sqlcmd = @"INSERT INTO Entries (TaskId, UserId, StartTime, EndTime, IsCompleted) OUTPUT INSERTED.EntryId VALUES (@TaskId, @UserId, @StartTime, @EndTime, @IsCompleted)";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var entryId = conn.QuerySingle<int>(sqlcmd, entry);
                    return entryId;
                }
            }
            catch (SqlException sqlex)
            {
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
            }
            return 0;
        }

        public bool UpdateEntry(Entry entry)
        {
            try
            {
                sqlcmd = @"UPDATE Entries SET TaskId = @TaskId, UserId = @UserId, StartTime = @StartTime, EndTime = @EndTime, IsCompleted = @IsCompleted WHERE EntryId = @EntryId";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, entry);
                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
            }
            return false;
        }

        public bool DeleteEntryById(int id)
        {
            try
            {
                sqlcmd = "DELETE FROM Entries WHERE EntryId = @Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, new { Id = id });
                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                Fehlermeldung = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                Fehlermeldung = $"Unexpected Error: {ex.Message}";
            }
            return false;
        }
    }
}