using Dapper;
using Microsoft.Data.SqlClient;
using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;
        public string sqlcmd;
        public string Fehlermeldung { get; set; }

        public TaskRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Atasko")!;
        }

        public List<TaskItem> GetAllTasks()
        {
            try
            {
                List<TaskItem> tasks = new List<TaskItem>();
                sqlcmd = "SELECT TaskId, Name, Interval FROM Tasks";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    tasks = conn.Query<TaskItem>(sqlcmd).ToList();
                    return tasks;
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
            return new List<TaskItem>();
        }

        public TaskItem? GetTaskById(int id)
        {
            try
            {
                TaskItem? task = null;
                sqlcmd = "SELECT TaskId, Name, Interval FROM Tasks WHERE TaskId = @Id";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    task = conn.QuerySingleOrDefault<TaskItem>(sqlcmd, new { Id = id });
                    return task;
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

        public int AddTask(TaskItem task)
        {
            try
            {
                sqlcmd = @"INSERT INTO Tasks (Name, Interval) OUTPUT INSERTED.TaskId VALUES (@Name, @Interval)";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var taskId = conn.QuerySingle<int>(sqlcmd, new { task.Name, Interval = (int)task.Interval });
                    return taskId;
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

        public bool UpdateTask(TaskItem task)
        {
            try
            {
                sqlcmd = @"UPDATE Tasks SET Name = @Name, Interval = @Interval WHERE TaskId = @TaskId";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, new { task.TaskId, task.Name, Interval = (int)task.Interval });
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

        public bool DeleteTaskById(int id)
        {
            try
            {
                sqlcmd = "DELETE FROM Tasks WHERE TaskId = @Id";
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