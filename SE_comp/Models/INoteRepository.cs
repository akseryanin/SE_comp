using System.Collections.Generic;
using System.Threading.Tasks;

namespace SE_comp.Models
{
    public interface INoteRepository
    {
        Task<IList<Note>> GetAllAsync();

        Task<Note> GetNoteByIdAsync(int id);

        Task<Note> CreateNewNoteAsync(Note note);
        
        Task<Note> UpdateNoteAsync(int id, NoteRequest note);

        Task<IList<Note>> SearchQueryAsync(string search);

        Task<bool> DeleteByIdAsync(int id);
    }
}