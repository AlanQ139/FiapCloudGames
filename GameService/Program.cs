using FiapCloudGames.Services;
using GameService.Data;
using GameService.Interfaces;
using GameService.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<GameDbContext>(options =>
//    options.UseInMemoryDatabase("FiapCloudGamesDev"));

// Banco em memória (trocar depois se quiser por SQLite ou SQL Server)
//builder.Services.AddDbContext<GameDbContext>(options =>
//    options.UseInMemoryDatabase("GamesDb"));
//    options.UseInMemoryDatabase("FiapCloudUsersDev"));
//,mudando de InMemory para SQL Server LocalDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IGameRepositories, GameRepository>();
//builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IGameService, GameServices>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

//teste de comunicação local
var usersUrl = builder.Configuration["Services:UserService"];
var paymentsUrl = builder.Configuration["Services:PaymentService"];

builder.Services.AddHttpClient("UsersClient", client =>
{
    client.BaseAddress = new Uri(usersUrl);
});

builder.Services.AddHttpClient("PaymentsClient", client =>
{
    client.BaseAddress = new Uri(paymentsUrl);
});


var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//    dbContext.Database.Migrate();
//}

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();