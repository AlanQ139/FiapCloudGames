using GameService.Data;
using GameService.DTOs;
using GameService.Interfaces;
using GameService.Models;
using GameService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameService.Data;
using GameService.Models;
using GameService.Repositories;

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

        // POST api/games/create (agora usa o token JWT)
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            // 1️ Extrai o ID do usuário autenticado do token
            //var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("userId"); // fallback se o UserService usar esse nome
                        
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token inválido: sem ID de usuário.");

            // 2️ Valida se o usuário existe no UserService
            var user = await _userClient.GetUserByIdAsync(Guid.Parse(userId));
            if (user == null)
                return BadRequest("Usuário não encontrado no UserService.");

            // 3️ Cria o jogo normalmente
            var game = new Game
            {
                Nome = request.GameName,
                Categoria = request.Category,
                Descricao = request.Description,
                Preco = request.Price,
                DataDeCadastro = DateTime.UtcNow
            };

            await _repository.AddAsync(game);

            return Ok(new
            {
                Message = $"Jogo '{request.GameName}' cadastrado com sucesso para o usuário {user.Nome}.",
                UserId = userId
            });
        }

        //GET /api/Games/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var game = await _repository.GetByIdAsync(id);
            if (game == null)
                return NotFound("Jogo inválido ou não encontrado");
            return Ok(game);
        }
    }

    public class CreateGameRequest
    {
        public string GameName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}

