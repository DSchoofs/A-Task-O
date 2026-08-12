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

        bool TakeOverBlock(int blockId, int userId);

        bool CompleteBlock(int blockId, int userId);

        bool ReleaseBlock(int blockId, int userId);

        string ErrorMessage { get; set; }
    }
}