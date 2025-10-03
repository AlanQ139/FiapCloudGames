// FiapCloudGames.Api/Models/Game.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GameService.Models
{    
    public class Game
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Categoria { get; set; }

        [Precision(18, 2)]
        public decimal Preco { get; set; }
        public DateTime DataDeCadastro { get; set; } = DateTime.UtcNow;
    }
}
