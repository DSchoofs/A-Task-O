using Schoofs_ATaskO.Models;

namespace Schoofs_ATaskO.Repositories
{
    public interface IEntryRepository
    {
        List<Entry> GetAllEntries();
        Entry? GetEntryById(int id);
        bool AddEntry(Entry entry);
        bool UpdateEntry(Entry entry);
        bool DeleteEntryById(int id);
        string ErrorMessage { get; set; }
    }
}