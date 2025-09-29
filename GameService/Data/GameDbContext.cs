using GameService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GameService.Data
{
    //public class GameDbContext : DbContext
    //{
    //    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    //    public DbSet<Game> Games { get; set; }
    //    public DbSet<UserGame> UserGames { get; set; }

    //    protected override void OnModelCreating(ModelBuilder modelBuilder)
    //    {
    //        // Configuração da chave primária composta para UserGame
    //        modelBuilder.Entity<UserGame>()
    //            .HasKey(ug => new { ug.UserId, ug.GameId });

    //        // Configuração do relacionamento para Game
    //        modelBuilder.Entity<UserGame>()
    //            .HasOne(ug => ug.Game)
    //            .WithMany(g => g.Usuarios)
    //            .HasForeignKey(ug => ug.GameId);

    //        // Configuração do relacionamento para User (será integrado depois)
    //        // modelBuilder.Entity<UserGame>()
    //        //     .HasOne(ug => ug.User)
    //        //     .WithMany() // ou WithMany(u => u.Biblioteca) se User tiver uma propriedade de navegação
    //        //     .HasForeignKey(ug => ug.UserId);
    //    }
    //}

    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<Game> Games { get; set; }
    }
}
