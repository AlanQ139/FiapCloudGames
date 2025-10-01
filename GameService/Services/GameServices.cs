using GameService.DTOs;
using GameService.Interfaces;
using GameService.Models;

namespace FiapCloudGames.Services
{
    public class GameServices : IGameService
    {
        private readonly IGameRepositories _repo;

        public GameServices(IGameRepositories repo) => _repo = repo;

        public async Task<IEnumerable<GameResponseDto>> GetAllAsync() =>
            (await _repo.GetAllAsync())
                .Select(g => new GameResponseDto
                {
                    Id = g.Id,
                    Nome = g.Nome,
                    Categoria = g.Categoria,
                    Descricao = g.Descricao,
                    Preco = g.Preco,
                    DataDeCadastro = g.DataDeCadastro
                });

        public async Task<GameResponseDto?> GetByIdAsync(Guid id)
        {
            var game = await _repo.GetByIdAsync(id);
            return game == null ? null : new GameResponseDto
            {
                Id = game.Id,
                Nome = game.Nome,
                Categoria = game.Categoria,
                Descricao = game.Descricao,
                Preco = game.Preco,
                DataDeCadastro = game.DataDeCadastro
            };
        }

        public async Task<GameResponseDto> CreateAsync(GameCreateDto dto)
        {
            var game = new Game
            {
                Nome = dto.Nome,
                Categoria = dto.Categoria,
                Descricao = dto.Descricao,
                Preco = dto.Preco
            };

            await _repo.AddAsync(game);

            return new GameResponseDto
            {
                Id = game.Id,
                Nome = game.Nome,
                Categoria = game.Categoria,
                Descricao = game.Descricao,
                Preco = game.Preco,
                DataDeCadastro = game.DataDeCadastro
            };
        }

        public async Task<GameResponseDto?> UpdateAsync(Guid id, GameUpdateDto dto)
        {
            var game = await _repo.GetByIdAsync(id);
            if (game == null) return null;

            game.Nome = dto.Nome;
            game.Categoria = dto.Categoria;
            game.Descricao = dto.Descricao;
            game.Preco = dto.Preco;
            game.DataDeCadastro = DateTime.UtcNow;

            await _repo.UpdateAsync(game);

            return new GameResponseDto
            {
                Id = game.Id,
                Nome = game.Nome,
                Categoria = game.Categoria,
                Descricao = game.Descricao,
                Preco = game.Preco,
                DataDeCadastro = game.DataDeCadastro
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var game = await _repo.GetByIdAsync(id);
            if (game == null) return false;
            await _repo.DeleteAsync(game);
            return true;
        }
    }
}
