using GameService.Data;
using GameService.Interfaces;
using GameService.Models;
using Microsoft.EntityFrameworkCore;

namespace GameService.Repositories
{
    public class GameRepository : IGameRepositories
    {
        private readonly GameDbContext _context;

        public GameRepository(GameDbContext context) => _context = context;
                
        public async Task<IEnumerable<Game>> GetAllAsync() =>
            await _context.Games.ToListAsync();

        public async Task<Game?> GetByIdAsync(Guid id) =>
            await _context.Games.FindAsync(id);

        public async Task AddAsync(Game game)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Game game)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }
    }
}
