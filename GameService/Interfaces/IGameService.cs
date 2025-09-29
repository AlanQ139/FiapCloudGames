using GameService.DTOs;

namespace GameService.Interfaces
{
    public interface IGameService
    {
        Task<IEnumerable<GameResponseDto>> GetAllAsync();
        Task<GameResponseDto?> GetByIdAsync(Guid id);
        Task<GameResponseDto> CreateAsync(GameCreateDto dto);
        Task<GameResponseDto?> UpdateAsync(Guid id, GameUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
