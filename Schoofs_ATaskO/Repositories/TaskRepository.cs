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
            _connectionString = configuration.GetConnectionString("Atasko")!;
        }

        public List<TaskItem> GetAllTasks()
        {
            try
            {
                List<TaskItem> tasks = new List<TaskItem>();
                sqlcmd = "SELECT TaskId, TaskName, TaskInterval FROM Tasks";
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
                sqlcmd = "SELECT TaskId, TaskName, TaskInterval FROM Tasks WHERE TaskId = @Id";
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
                sqlcmd = @"INSERT INTO Tasks (TaskName, TaskInterval) OUTPUT INSERTED.TaskId VALUES (@TaskName, @TaskInterval)";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, new { task.TaskName, TaskInterval = (int)task.TaskInterval });
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
                sqlcmd = @"UPDATE Tasks SET TaskName = @TaskName, TaskInterval = @TaskInterval WHERE TaskId = @TaskId";
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    var rowsAffected = conn.Execute(sqlcmd, new { task.TaskId, task.TaskName, TaskInterval = (int)task.TaskInterval });
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