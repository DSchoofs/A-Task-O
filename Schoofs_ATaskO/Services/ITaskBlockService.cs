using Schoofs_ATaskO.ViewModels;

namespace Schoofs_ATaskO.Services
{
    public interface ITaskBlockService
    {
        TaskViewViewModel? GetTaskView(int taskItemId, DateTime focusDate, int? selectedBlockId = null);
        bool GenerateBlocks(int taskItemId, DateTime focusDate);
        bool TakeOverBlock(int blockId);
        bool CompleteBlock(int blockId);
        string ErrorMessage { get; }
    }
}