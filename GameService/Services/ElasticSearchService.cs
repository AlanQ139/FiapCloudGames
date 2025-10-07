using Nest;
using GameService.Models;
using GameService.DTOs;

namespace GameService.Services
{
    public class ElasticSearchService
    {
        private readonly IElasticClient _client;
        private const string IndexName = "games";

        public ElasticSearchService(IConfiguration configuration)
        {
            //var settings = new ConnectionSettings(new Uri(configuration["ElasticSearch:Url"]))
            //    .DefaultIndex(IndexName);
            var url = configuration["ElasticSearch:Url"];
            var username = configuration["ElasticSearch:Username"];
            var password = configuration["ElasticSearch:Password"];

            var settings = new ConnectionSettings(new Uri(url))
            .DefaultIndex(IndexName)
            .BasicAuthentication(username, password); // importante para cloud

            _client = new ElasticClient(settings);

            // Cria o índice se não existir
            try
            {
                var existsResponse = _client.Indices.Exists(IndexName);
                if (!existsResponse.Exists)
                {
                    _client.Indices.Create(IndexName, c => c.Map<Game>(m => m.AutoMap()));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar índice Elasticsearch: {ex.Message}");
                throw;
            }
        }

        public async Task IndexGameAsync(Game game)
        {
            //await _client.IndexDocumentAsync(game);
            try
            {
                await _client.IndexDocumentAsync(game);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao indexar game: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<Game>> SearchGamesAsync(string category)
        {
            var response = await _client.SearchAsync<Game>(s => s
                .Query(q => q.Match(m => m.Field(f => f.Categoria).Query(category)))
                .Size(3)
            );

            return response.Documents;
        }

        public async Task<GameMetricsDto> GetGameMetricsAsync()
        {
            var response = await _client.SearchAsync<Game>(s => s
                .Size(0) // não queremos documentos, só agregações
                .Aggregations(a => a
                    .Terms("categorias", t => t
                        .Field(f => f.Categoria.Suffix("keyword"))
                        .Size(10)
                    )
                    .Average("preco_medio", avg => avg.Field(f => f.Preco))
                )
            );

            if (!response.IsValid)
            {
                // logar e retornar vazio em caso de erro
                // (use ILogger no serviço para logar response.DebugInformation)
                return new GameMetricsDto();
            }

            var termos = response.Aggregations.Terms("categorias");
            var categorias = termos?.Buckets.Select(b => new CategoryMetric
            {
                Categoria = b.Key,
                Quantidade = b.DocCount ?? 0
            }) ?? Enumerable.Empty<CategoryMetric>();

            var precoMedio = response.Aggregations.Average("preco_medio")?.Value;

            return new GameMetricsDto
            {
                PorCategoria = categorias,
                PrecoMedio = precoMedio
            };
        }
    }
}
