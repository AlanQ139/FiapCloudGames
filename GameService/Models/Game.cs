// FiapCloudGames.Api/Models/Game.cs
using System;
using System.Collections.Generic;

namespace GameService.Models
{    
    public class Game
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }
        public decimal Preco { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}
