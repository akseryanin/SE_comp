using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SE_comp.Models;

namespace SE_comp.Controllers
{
    [Produces("application/json")]
    [Route("notes")]
    public class NotesController : Controller
    {
        private readonly INoteRepository _repository;
        private readonly MyOption _option;

        public NotesController(INoteRepository repository, IOptions<MyOption> opt)
        {
            _repository = repository;
            _option = opt.Value;
        }

        /// <summary>
        /// Получение списка всех заметок
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Note>), 200)]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetNotes([FromQuery] string searchString)
        {
            try
            {
                var notes = string.IsNullOrEmpty(searchString)
                    ? await _repository.GetAllAsync()
                    : await _repository.SearchQueryAsync(searchString);
                
                if (notes.Count == 0)
                {
                    return StatusCode(StatusCodes.Status204NoContent, null);
                }

                if (HttpContext.Request.Headers.ContainsKey(_option.Header))
                {
                    return Ok(notes);
                }
                else
                {
                    var note = notes.First();
                    var contentLength = Math.Min(_option.Num, note.Content.Length);
                    return Ok(note.Content.Substring(0, contentLength));
                }
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something gone wrong");
            }
        }
        
        /// <summary>
        /// Создание новой заметки
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Note), 201)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> CreateNewNote([FromBody] NoteRequest note)
        {
            try
            {
                var newNote = await _repository.CreateNewNoteAsync(new Note
                {
                    Title = note.Title,
                    Content = note.Content
                });

                return StatusCode(StatusCodes.Status201Created, newNote);
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something has gone wrong");
            }
        }

        /// <summary>
        /// Редактирование заметки
        /// </summary>
        /// <param name="id"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Note), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> EditNote([FromRoute] int id, [FromBody] NoteRequest note)
        {
            try
            {
                var newNote = await _repository.UpdateNoteAsync(id, note);

                return Ok(newNote);
            }
            catch (DataException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something has gone wrong");
            }
        }

        /// <summary>
        /// Получение одной заметки по id
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(List<Note>), 200)]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 204)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> GetNotesById([FromRoute] int id)
        {
            try
            {
                var note = await _repository.GetNoteByIdAsync(id);
                if (note == null)
                {
                    return StatusCode(StatusCodes.Status204NoContent, null);
                }

                if (HttpContext.Request.Headers.ContainsKey(_option.Header))
                {
                    return Ok(note);
                }
                else
                {
                    var contentLength = Math.Min(_option.Num, note.Content.Length);
                    return Ok(note.Content.Substring(0, contentLength));
                }
            }
            catch (DataException exc)
            {
                return StatusCode(StatusCodes.Status204NoContent, null);
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something has gone wrong");
            }
        }
        
        /// <summary>
        /// Удаление заметки по идентификатору
        /// </summary>
        /// <param name="id"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public async Task<IActionResult> DeleteNote([FromRoute] int id)
        {
            try
            {
                var result = await _repository.DeleteByIdAsync(id);

                return Ok(result);
            }
            catch (Exception exc)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something has gone wrong");
            }
        }
    }
}