using Dapper;
using Microsoft.Data.SqlClient;
using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;
        public string sqlcmd = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public TaskRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ATaskO")!;
        }

        public List<TaskItem> GetAllTasks()
        {
            try
            {
                List<TaskItem> tasks = new List<TaskItem>();
                sqlcmd = "SELECT Id, Name, Description, Interval, WeekMode, IsActive, CreatedAt, CreatedByUserId, ModifiedAt, ModifiedByUserId FROM TaskItems"; 
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    tasks = conn.Query<TaskItem>(sqlcmd).ToList();
                    return tasks;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }
            return new List<TaskItem>();
        }

        public TaskItem? GetTaskById(int id)
        {
            try
            {
                TaskItem? task = null;
                sqlcmd = "SELECT Id, Name, Description, Interval, WeekMode, IsActive, CreatedAt, CreatedByUserId, ModifiedAt, ModifiedByUserId FROM TaskItems WHERE Id = @Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    task = conn.QuerySingleOrDefault<TaskItem>(sqlcmd, new { Id = id });
                    return task;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }
            return null;
        }

        public bool AddTask(TaskItem task)
        {
            try
            {
                sqlcmd = "INSERT INTO TaskItems (Name, Description, Interval, WeekMode, IsActive, CreatedAt, CreatedByUserId) VALUES (@Name, @Description, @Interval, @WeekMode, @IsActive, @CreatedAt, @CreatedByUserId);";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    var parameters = new
                    {
                        task.Name,
                        task.Description,
                        Interval = (int)task.Interval,
                        WeekMode = (int)task.WeekMode,
                        task.IsActive,
                        task.CreatedAt,
                        task.CreatedByUserId
                    };

                    var rowsAffected = conn.Execute(sqlcmd, parameters);
                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }
            return false;
        }

        public bool UpdateTask(TaskItem task)
        {
            try
            {
                sqlcmd = "UPDATE TaskItems SET Name = @Name, Description = @Description, Interval = @Interval, WeekMode = @WeekMode, IsActive = @IsActive, ModifiedAt = @ModifiedAt, ModifiedByUserId = @ModifiedByUserId WHERE Id = @Id;";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    var parameters = new
                    {
                        task.Id,
                        task.Name,
                        task.Description,
                        Interval = (int)task.Interval,
                        WeekMode = (int)task.WeekMode,
                        task.IsActive,
                        task.ModifiedAt,
                        task.ModifiedByUserId
                    };

                    var rowsAffected = conn.Execute(sqlcmd, parameters);
                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }
            return false;
        }

        public bool DeleteTaskWithBlocksById(int id)
        {
            try
            {
                sqlcmd = "DELETE FROM TaskBlocks WHERE TaskItemId = @Id";

                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        conn.Execute(sqlcmd,new { Id = id },transaction);

                        sqlcmd = "DELETE FROM TaskItems WHERE Id = @Id";

                        var rowsAffected = conn.Execute(sqlcmd,new { Id = id },transaction);

                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();

                            ErrorMessage = "Die Aufgabe wurde nicht gefunden.";

                            return false;
                        }

                        transaction.Commit();

                        return true;
                    }
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error: {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error: {ex.Message}";
            }

            return false;
        }
    }
}