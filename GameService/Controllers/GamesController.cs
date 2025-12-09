using GameService.Data;
using GameService.Data;
using GameService.DTOs;
using GameService.Interfaces;
using GameService.Models;
using GameService.Models;
using GameService.Repositories;
using GameService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GameService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameRepositories _repository;
        private readonly AppDbContext _context;
        //private readonly UserClient _userClient;
        //private readonly ElasticSearchService _elastic;
        private readonly IPublishEndpoint _publishEndpoint; // MassTransit publisher
        private readonly ILogger<GamesController> _logger;

        //public GamesController(IGameRepositories repository, AppDbContext context, UserClient userClient, ElasticSearchService elastic)
        //{
        //    _repository = repository;
        //    _context = context;
        //    _userClient = userClient;
        //    _elastic = elastic;
        //}

        public GamesController(IGameRepositories repository, AppDbContext context, IPublishEndpoint publishEndpoint, ILogger<GamesController> logger)
        {
            _repository = repository;
            _context = context;
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

        // POST api/games/{gameId}/buy?userId=123
        //[HttpPost("{gameId}/buy")]
        //public async Task<IActionResult> Buy(Guid gameId, [FromQuery] Guid userId)
        //{
        //    var game = await _repository.GetByIdAsync(gameId);
        //    if (game == null)
        //        return NotFound("Jogo não encontrado.");

        //    var purchase = new Purchase
        //    {
        //        GameId = gameId,
        //        UserId = userId
        //    };

        //    _context.Purchases.Add(purchase);
        //    await _context.SaveChangesAsync();

        //    return Ok(purchase);
        //}

        /// <summary>
        /// Comprar jogo - VERSÃO MASSTRANSIT
        /// Muito mais simples que a versão RabbitMQ pura!
        /// </summary>
        [HttpPost("{gameId}/buy")]
        [Authorize] // Usuário precisa estar autenticado
        public async Task<IActionResult> BuyGame(Guid gameId, [FromQuery] Guid userId)
        {
            // 1. Valida se o jogo existe
            var game = await _repository.GetByIdAsync(gameId);
            if (game == null)
                return NotFound(new { error = "Jogo não encontrado." });

            // 2. Cria registro de compra com status "Pending"
            var purchase = new Purchase
            {
                GameId = gameId,
                UserId = userId,
                PurchaseDate = DateTime.UtcNow
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            try
            {
                // 3. 🚀 PUBLICA EVENTO usando MassTransit
                // Muito mais simples! Sem precisar criar ConnectionFactory, Channel, etc.
                await _publishEndpoint.Publish<IGamePurchased>(new
                {
                    PurchaseId = purchase.Id,
                    UserId = userId,
                    GameId = gameId,
                    GamePrice = game.Preco,
                    PurchaseDate = purchase.PurchaseDate
                });

                _logger.LogInformation(
                    "🎮 Compra publicada via MassTransit: Purchase={PurchaseId}, User={UserId}, Game={GameId}",
                    purchase.Id, userId, gameId);

                return Accepted(new
                {
                    message = "Compra registrada! O pagamento será processado em breve.",
                    purchaseId = purchase.Id,
                    status = "Pending"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento de compra via MassTransit");

                // Remove a compra se não conseguiu publicar o evento
                _context.Purchases.Remove(purchase);
                await _context.SaveChangesAsync();

                return StatusCode(500, new
                {
                    error = "Erro ao processar compra. Tente novamente.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var game = await _repository.GetByIdAsync(id);
            if (game == null)
                return NotFound("Jogo inválido ou não encontrado");
            return Ok(game);
        }

        // GET api/games/recommend?category=RPG
        //[HttpGet("recommend")]
        //public async Task<IActionResult> Recommend([FromQuery] string category)
        //{
        //    var recommended = await _elastic.SearchGamesAsync(category);
        //    return Ok(recommended);
        //}

        // POST api/games/create (agora usa o token JWT)
        //[Authorize]
        //[HttpPost("create")]
        //public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
        //{
        //    // 1️ Extrai o ID do usuário autenticado do token
        //    var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        //    ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
        //    ?? User.FindFirstValue("userId"); // fallback se o UserService usar esse nome
                        
        //    if (string.IsNullOrEmpty(userId))
        //        return Unauthorized("Token inválido: sem ID de usuário.");

        //    // 2️ Valida se o usuário existe no UserService
        //    var user = await _userClient.GetUserByIdAsync(Guid.Parse(userId));
        //    if (user == null)
        //        return BadRequest("Usuário não encontrado no UserService.");

        //    // 3️ Cria o jogo normalmente
        //    var game = new Game
        //    {
        //        Nome = request.GameName,
        //        Categoria = request.Category,
        //        Descricao = request.Description,
        //        Preco = request.Price,
        //        DataDeCadastro = DateTime.UtcNow
        //    };

        //    await _repository.AddAsync(game);
        //    //para o elasticsearch
        //    await _elastic.IndexGameAsync(game);

        //    return Ok(new
        //    {
        //        Message = $"Jogo '{request.GameName}' cadastrado com sucesso para o usuário {user.Nome}.",
        //        UserId = userId
        //    });
        //}

        //GET /api/Games/{id}
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetById(Guid id)
        //{
        //    var game = await _repository.GetByIdAsync(id);
        //    if (game == null)
        //        return NotFound("Jogo inválido ou não encontrado");
        //    return Ok(game);
        //}

        //[HttpGet("metrics")]
        //public async Task<IActionResult> GetGameMetrics()
        //{
        //    var metrics = await _elastic.GetGameMetricsAsync();
        //    return Ok(metrics);
        //}

    }

    public class CreateGameRequest
    {
        public string GameName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}

