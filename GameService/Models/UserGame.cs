// FiapCloudGames.Api/Models/UserGame.cs
using System;

namespace GameService.Models
{    
    public class UserGame
    {
        public Guid UserId { get; set; }
        // public User User { get; set; } // Descomentar quando integrar com FiapCloudUsers

        public Guid GameId { get; set; }
        public Game Game { get; set; }

        public DateTime DataAquisicao { get; set; } = DateTime.UtcNow;
    }
}
