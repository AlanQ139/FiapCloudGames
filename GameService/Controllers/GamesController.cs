using GameService.Data;
using GameService.DTOs;
using GameService.Interfaces;
using GameService.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameRepositories _repository;
        private readonly GameDbContext _context;

        public GamesController(IGameRepositories repository, GameDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        // GET api/games
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var games = await _repository.GetAllAsync();
            return Ok(games);
        }

        // POST api/games
        [HttpPost]
        public async Task<IActionResult> Add(Game game)
        {
            await _repository.AddAsync(game);
            return Ok(game);
        }

        // POST api/games/{gameId}/buy?userId=123
        [HttpPost("{gameId}/buy")]
        public async Task<IActionResult> Buy(Guid gameId, [FromQuery] Guid userId)
        {
            var game = await _repository.GetByIdAsync(gameId);
            if (game == null)
                return NotFound("Jogo não encontrado.");

            var purchase = new Purchase
            {
                GameId = gameId,
                UserId = userId
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return Ok(purchase);
        }

        // GET api/games/recommend?category=RPG
        [HttpGet("recommend")]
        public IActionResult Recommend([FromQuery] string category)
        {
            var recommended = _context.Games
                .Where(g => g.Categoria == category)
                .Take(3)
                .ToList();

            return Ok(recommended);
        }
    }
}
