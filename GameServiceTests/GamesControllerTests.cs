using Xunit;
using Moq;
using GameService.Controllers;
using GameService.Interfaces;
using GameService.Data;
using GameService.Models;
using GameService.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using GameService.DTOs;

namespace GameServiceTests
{
    public class GamesControllerTests
    {
        private readonly Mock<IGameRepositories> _repoMock = new();
        private readonly Mock<AppDbContext> _contextMock = new();
        private readonly Mock<UserClient> _userClientMock = new();
        private readonly Mock<ElasticSearchService> _elasticMock = new();

        private GamesController CreateControllerWithUser(ClaimsPrincipal? user = null)
        {
            var controller = new GamesController(
                _repoMock.Object,
                _contextMock.Object,
                _userClientMock.Object,
                _elasticMock.Object
            );
            if (user != null)
            {
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                };
            }
            return controller;
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithGames()
        {
            var games = new List<Game> { new Game { Nome = "Test" } };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(games);

            var controller = CreateControllerWithUser();
            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(games, okResult.Value);
        }

        [Fact]
        public async Task Add_ReturnsOkWithGame()
        {
            var game = new Game { Nome = "Test" };
            _repoMock.Setup(r => r.AddAsync(game)).Returns(Task.CompletedTask);

            var controller = CreateControllerWithUser();
            var result = await controller.Add(game);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(game, okResult.Value);
        }

        [Fact]
        public async Task Buy_GameNotFound_ReturnsNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null);

            var controller = CreateControllerWithUser();
            var result = await controller.Buy(Guid.NewGuid(), Guid.NewGuid());

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Jogo não encontrado.", notFound.Value);
        }

        [Fact]
        public async Task Buy_GameFound_ReturnsOkWithPurchase()
        {
            var gameId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var game = new Game { Id = gameId };
            _repoMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync(game);

            var purchases = new List<Purchase>();
            _contextMock.Setup(c => c.Purchases).Returns(Mock.Of<Microsoft.EntityFrameworkCore.DbSet<Purchase>>());
            _contextMock.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            var controller = CreateControllerWithUser();
            var result = await controller.Buy(gameId, userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var purchase = Assert.IsType<Purchase>(okResult.Value);
            Assert.Equal(gameId, purchase.GameId);
            Assert.Equal(userId, purchase.UserId);
        }

        [Fact]
        public async Task Recommend_ReturnsOkWithRecommendedGames()
        {
            var category = "RPG";
            var recommended = new List<Game> { new Game { Categoria = category } };
            _elasticMock.Setup(e => e.SearchGamesAsync(category)).ReturnsAsync(recommended);

            var controller = CreateControllerWithUser();
            var result = await controller.Recommend(category);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(recommended, okResult.Value);
        }

        [Fact]
        public async Task CreateGame_InvalidToken_ReturnsUnauthorized()
        {
            var controller = CreateControllerWithUser(new ClaimsPrincipal());
            var request = new CreateGameRequest();

            var result = await controller.CreateGame(request);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Token inválido: sem ID de usuário.", unauthorized.Value);
        }

        [Fact]
        public async Task CreateGame_UserNotFound_ReturnsBadRequest()
        {
            var userId = Guid.NewGuid().ToString();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            
            var controller = CreateControllerWithUser(principal);
            var request = new CreateGameRequest();

            var result = await controller.CreateGame(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Usuário não encontrado no UserService.", badRequest.Value);
        }

        [Fact]
        public async Task GetById_GameNotFound_ReturnsNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null);

            var controller = CreateControllerWithUser();
            var result = await controller.GetById(Guid.NewGuid());

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Jogo inválido ou não encontrado", notFound.Value);
        }

        [Fact]
        public async Task GetById_GameFound_ReturnsOkWithGame()
        {
            var game = new Game { Id = Guid.NewGuid() };
            _repoMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

            var controller = CreateControllerWithUser();
            var result = await controller.GetById(game.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(game, okResult.Value);
        }

        [Fact]
        public async Task GetGameMetrics_ReturnsOkWithMetrics()
        {
            var metrics = new GameMetricsDto();
            _elasticMock.Setup(e => e.GetGameMetricsAsync()).ReturnsAsync(metrics);

            var controller = CreateControllerWithUser();
            var result = await controller.GetGameMetrics();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(metrics, okResult.Value);
        }
    }
}
