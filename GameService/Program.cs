using FiapCloudGames.Services;
using GameService.Data;
using GameService.Interfaces;
using GameService.Repositories;
using GameService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using GameService.Middleware;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Swagger com definição Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GameService", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Cole aqui o token JWT (somente o token, sem 'Bearer ')."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] {}
        }
    });
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repos e serviços
builder.Services.AddScoped<IGameRepositories, GameRepository>();
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

// ============ RABBITMQ + MASSTRANSIT ============
builder.Services.AddMassTransit(x =>
{
    // UserService publica eventos, não consome
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });

        cfg.ConfigureEndpoints(context);

        cfg.UseMessageRetry(r => r.Incremental(
            retryLimit: 3,
            initialInterval: TimeSpan.FromSeconds(1),
            intervalIncrement: TimeSpan.FromSeconds(2)
        ));
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<ElasticSearchService>();

// HttpClient + handler para propagar token
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();

builder.Services.AddHttpClient<UserClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["USERS_URL"] ?? "https://localhost:7126");
})
.AddHttpMessageHandler<BearerTokenHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    int retries = 0;
    const int maxRetries = 10;

    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            retries++;
            if (retries >= maxRetries)
                throw;
            Console.WriteLine($"Banco ainda não pronto... tentando novamente ({retries}/{maxRetries})");
            Thread.Sleep(3000);
        }
    }
}

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

#region Middleware customizado
app.UseMiddleware<ErrorHandlingMiddleware>();
#endregion
// Ordem correta de middlewares
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();