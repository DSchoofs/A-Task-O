using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public interface ITaskBlockRepository
    {
        List<TaskBlock> GetAllBlocks();

        List<TaskBlock> GetBlocksByTaskItemId(int taskItemId);

        TaskBlock? GetBlockById(int id);

        int AddBlock(TaskBlock block);

        bool UpdateBlock(TaskBlock block);

        bool DeleteBlockById(int id);

        string ErrorMessage { get; set; }
    }
}