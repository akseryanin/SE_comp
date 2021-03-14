using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SE_comp.Models
{
    /// <summary>
    /// Кастомная бд - на основе потокобезопасного словаря
    /// </summary>
    public class NoteRepository : INoteRepository
    {
        private readonly ConcurrentDictionary<int, Note> db;
        private int counter; 
        
        public NoteRepository()
        {
            db = new ConcurrentDictionary<int, Note>();
            counter = 0;
        }

        public Task<IList<Note>> GetAllAsync() => Task
            .FromResult<IList<Note>>(db.Values.ToList());

        public Task<Note> GetNoteByIdAsync(int id)
        {
            if (db.TryGetValue(id, out var result))
            {
                return Task.FromResult(result);
            }

            throw new DataException("Failed to find by id");
        }

        public Task<Note> CreateNewNoteAsync(Note note)
        {
            note.Id = counter;
            ++counter;
            // Workaround
            db.AddOrUpdate(note.Id, note, (_, note1) => note1);
            return Task.FromResult(note);
        }
        
        public async Task<Note> UpdateNoteAsync(int id, NoteRequest NoteR)
        {
            var note = await GetNoteByIdAsync(id);
            if (note == null)
                throw new DataException("No such note");
            return db.AddOrUpdate(note.Id, note, (_, note1) =>
            {
                note1.Content = NoteR.Content;
                note1.Title = NoteR.Title;
                return note1;
            });
        }

        public Task<bool> DeleteByIdAsync(int id)
        {
            if (db.TryRemove(id, out var _))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<IList<Note>> SearchQueryAsync(string search)
        {
            var result = new List<Note>();
            
            foreach (var key in db.Keys)
            {
                if (db.TryGetValue(key, out var note))
                {
                    if (note.Content.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || note.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(note);
                    }
                }
            }
            
            return Task.FromResult<IList<Note>>(result);
        }
    }
}