using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EMT.BLL.Services;
using EMT.Common;
using EMT.Common.Entities;
using EMT.API.ApiUtils;
using System;

namespace EMT.API.Controllers
{
    public class MyNoteController: BaseApiController
    {
        private readonly IMyNoteService _service;

        public MyNoteController(IOptionsSnapshot<MyAppConfig> myAppConfig, ILogger<MyNoteController> logger, IMyNoteService service) 
                                        : base(myAppConfig, logger)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(uint id)
        {
            var response = await _service.GetById(id);
            if (response.Data == null) return NotFound();
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> Get(string search = null, int page = 1, int pageSize = 5)
        {
            var response = await _service.GetSearchAndPaginated(search, page, pageSize);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MyNote newEntity)
        {
            var response = await _service.Create(newEntity);
            return CreatedAtAction(nameof(Get), new { id = newEntity.NoteId }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(uint id, [FromBody] MyNote updatedEntity)
        {
            var response = await _service.Update(id, updatedEntity);
            if (response.Data == null) return NotFound();
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(uint id)
        {
            var response = await _service.DeleteById(id);
            if (response.Data == null) return NotFound();
            return NoContent();
        }

    }
}
