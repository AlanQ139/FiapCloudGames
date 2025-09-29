//using GameService.Data;

//var builder = WebApplication.CreateBuilder(args);

////builder.AddDbContext<GameDbContext>(options =>
////    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();


//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}


//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();

using FiapCloudGames.Services;
using GameService.Data;
using GameService.Interfaces;
using GameService.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseInMemoryDatabase("FiapCloudGamesDev"));

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