using GameService.Data;
using GameService.DTOs;
using GameService.Interfaces;
using GameService.Models;
using GameService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MassTransit;
using Shared.Contracts;

namespace GameService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameRepositories _repository;
        private readonly AppDbContext _context;
        private readonly UserClient _userClient;
        private readonly ElasticSearchService _elastic; // para o ELASTICSEARCH
        private readonly IPublishEndpoint _publishEndpoint; // para o MassTransit para eventos
        private readonly ILogger<GamesController> _logger;

        public GamesController(
            IGameRepositories repository,
            AppDbContext context,
            UserClient userClient,
            ElasticSearchService elastic,
            IPublishEndpoint publishEndpoint,
            ILogger<GamesController> logger)
        {
            _repository = repository;
            _context = context;
            _userClient = userClient;
            _elastic = elastic;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
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

        // POST api/games/{gameId}/buy        
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

            // chamando o MassTransit ao invés de _rabbitMQ.PublishMessage
            try
            {
                await _publishEndpoint.Publish<IGamePurchased>(new
                {
                    PurchaseId = purchase.Id,
                    UserId = userId,
                    GameId = gameId,
                    GamePrice = game.Preco,
                    PurchaseDate = purchase.PurchaseDate
                });

                _logger.LogInformation(
                    "🎮 Compra publicada: Purchase={PurchaseId}, User={UserId}, Game={GameId}",
                    purchase.Id, userId, gameId);

                return Ok(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento de compra");
                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();
                return StatusCode(500, "Erro ao processar compra");
            }
        }

        // GET api/games/recommend?category=RPG
        // utiliza Elasticsearch para recomendações
        [HttpGet("recommend")]
        public async Task<IActionResult> Recommend([FromQuery] string category)
        {
            var recommended = await _elastic.SearchGamesAsync(category);
            return Ok(recommended);
        }

        // ============================================
        // POST api/games/create
        // ✨ MUDANÇA: Mantém lógica, adiciona evento ao Elasticsearch

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        {
            //pega ID do usuário do token
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("userId");

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Token inválido: sem ID de usuário.");

            //Valida usuário
            var user = await _userClient.GetUserByIdAsync(Guid.Parse(userId));
            if (user == null)
                return BadRequest("Usuário não encontrado no UserService.");

            // cria o jogo
            var game = new Game
            {
                Nome = request.GameName,
                Categoria = request.Category,
                Descricao = request.Description,
                Preco = request.Price,
                DataDeCadastro = DateTime.UtcNow
            };

            await _repository.AddAsync(game);

            // Indexa no Elasticsearch
            await _elastic.IndexGameAsync(game);

            return Ok(new
            {
                Message = $"Jogo '{request.GameName}' cadastrado com sucesso.",
                GameId = game.Id,
                UserId = userId
            });
        }

        // GET api/games/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var game = await _repository.GetByIdAsync(id);
            if (game == null)
                return NotFound("Jogo inválido ou não encontrado");
            return Ok(game);
        }

        // GET api/games/metrics
        [HttpGet("metrics")]
        public async Task<IActionResult> GetGameMetrics()
        {
            var metrics = await _elastic.GetGameMetricsAsync();
            return Ok(metrics);
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