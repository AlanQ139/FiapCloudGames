using FiapCloudGames.Services;
using GameService.Data;
using GameService.Interfaces;
using GameService.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<GameDbContext>(options =>
//    options.UseInMemoryDatabase("FiapCloudGamesDev"));

// Banco em memória (trocar depois se quiser por SQLite ou SQL Server)
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseInMemoryDatabase("GamesDb"));

builder.Services.AddScoped<IGameRepositories, GameRepository>();
//builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameService, GameServices>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();