using GameService.DTOs;
using GameService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _service;

        public GamesController(IGameService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var game = await _service.GetByIdAsync(id);
            return game == null ? NotFound() : Ok(game);
        }

        [HttpPost]
        public async Task<IActionResult> Create(GameCreateDto dto)
        {
            var game = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = game.Id }, game);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, GameUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id) =>
            await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
