using GameService.Models;

namespace GameService.Interfaces
{
    public interface IGameRepositories
    {
        Task<IEnumerable<Game>> GetAllAsync();
        Task<Game?> GetByIdAsync(Guid id);
        Task AddAsync(Game game);
        Task UpdateAsync(Game game);
        Task DeleteAsync(Game game);
    }
}
