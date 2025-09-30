namespace GameService.Models
{
    public class Purchase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }  // de onde vem o usuário (futuro UsersService)
        public Guid GameId { get; set; }  // jogo comprado
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    }
}
