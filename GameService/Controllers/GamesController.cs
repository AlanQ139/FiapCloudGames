using GameService.Data;
using GameService.DTOs;
using GameService.Interfaces;
using GameService.Models;
using GameService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameRepositories _repository;
        private readonly AppDbContext _context;
        private readonly UserClient _userClient;

        public GamesController(IGameRepositories repository, AppDbContext context, UserClient userClient)
        {
            _repository = repository;
            _context = context;
            _userClient = userClient;
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

        // POST api/games/create
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            // Valida usuário antes de criar o jogo
            var user = await _userClient.GetUserByIdAsync(request.UserId);

            if (user == null)
                return BadRequest("Usuário não encontrado");

            // Aqui faria o insert do jogo normalmente
            return Ok($"Jogo cadastrado para o usuário {user.Nome}");
        }
    }

    public class CreateGameRequest
    {
        public Guid UserId { get; set; }
        public string GameName { get; set; } = string.Empty;
    }
}
