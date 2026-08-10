using Dapper;
using Microsoft.Data.SqlClient;

namespace Schoofs_ATaskO.Repositories
{
    public class TaskBlockRepository : ITaskBlockRepository
    {
        private readonly string connectionString;

        public string sqlcmd { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public TaskBlockRepository(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("ATaskO")!;
        }

        public List<TaskBlock> GetAllBlocks()
        {
            try
            {
                List<TaskBlock> blocks = new List<TaskBlock>();

                sqlcmd = "SELECT Id, TaskItemId, Start, [End], Status, AssignedByUserId, AssignedAt, CompletedByUserId, CompletedAt, Note FROM TaskBlocks ORDER BY Start";

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    blocks = conn.Query<TaskBlock>(sqlcmd).ToList();

                    return blocks;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error:  {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error:  {ex.Message}";
            }

            return new List<TaskBlock>();
        }

        public List<TaskBlock> GetBlocksByTaskItemId(int taskItemId)
        {
            List<TaskBlock> blocks = new List<TaskBlock>();

            try
            {
                sqlcmd = "SELECT Id, TaskItemId, Start, [End], Status, AssignedByUserId, AssignedAt, CompletedByUserId, CompletedAt, Note FROM TaskBlocks WHERE TaskItemId = @TaskItemId ORDER BY Start";

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    blocks = conn.Query<TaskBlock>(sqlcmd, new{ TaskItemId = taskItemId }).ToList();
                    return blocks;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error:  {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error:  {ex.Message}";
            }

            return new List<TaskBlock>();
        }

        public TaskBlock? GetBlockById(int id)
        {
            try
            {
                TaskBlock? block = null;

                sqlcmd = "SELECT Id, TaskItemId, Start, [End], Status, AssignedByUserId, AssignedAt, CompletedByUserId, CompletedAt, Note FROM TaskBlocks WHERE Id = @Id";

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    block = conn.QuerySingleOrDefault<TaskBlock>(
                        sqlcmd,
                        new { Id = id });

                    return block;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error:  {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error:  {ex.Message}";
            }

            return null;
        }

        public int AddBlock(TaskBlock block)
        {
            try
            {
                sqlcmd = "INSERT INTO TaskBlocks (TaskItemId, Start, [End], Status, AssignedByUserId, AssignedAt, CompletedByUserId, CompletedAt, Note) OUTPUT INSERTED.Id VALUES (@TaskItemId, @Start, @End, @Status, @AssignedByUserId, @AssignedAt, @CompletedByUserId, @CompletedAt, @Note)";

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var blockId = conn.QuerySingle<int>( sqlcmd, new
                        {
                            block.TaskItemId,
                            block.Start,
                            block.End,
                            Status = (int)block.Status,
                            block.AssignedByUserId,
                            block.AssignedAt,
                            block.CompletedByUserId,
                            block.CompletedAt,
                            block.Note
                        });

                    return blockId;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error:  {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error:  {ex.Message}";
            }

            return 0;
        }

        public bool UpdateBlock(TaskBlock block)
        {
            try
            {
                sqlcmd = "UPDATE TaskBlocks SET TaskItemId = @TaskItemId, Start = @Start, [End] = @End, Status = @Status, AssignedByUserId = @AssignedByUserId, AssignedAt = @AssignedAt, CompletedByUserId = @CompletedByUserId, CompletedAt = @CompletedAt, Note = @Note WHERE Id = @Id";

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var rowsAffected = conn.Execute(sqlcmd, new
                        {
                            block.Id,
                            block.TaskItemId,
                            block.Start,
                            block.End,
                            Status = (int)block.Status,
                            block.AssignedByUserId,
                            block.AssignedAt,
                            block.CompletedByUserId,
                            block.CompletedAt,
                            block.Note
                        });

                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error:  {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error:  {ex.Message}";
            }

            return false;
        }

        public bool DeleteBlockById(int id)
        {
            try
            {
                sqlcmd = "DELETE FROM TaskBlocks WHERE Id = @Id";

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    var rowsAffected = conn.Execute(
                        sqlcmd,
                        new { Id = id });

                    return rowsAffected > 0;
                }
            }
            catch (SqlException sqlex)
            {
                ErrorMessage = $"SQL Error:  {sqlex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Unexpected Error:  {ex.Message}";
            }

            return false;
        }
    }
}