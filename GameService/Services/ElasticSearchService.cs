using Nest;
using GameService.Models;

namespace GameService.Services
{
    public class ElasticSearchService
    {
        private readonly IElasticClient _client;
        private const string IndexName = "games";

        public ElasticSearchService(IConfiguration configuration)
        {
            var settings = new ConnectionSettings(new Uri(configuration["ElasticSearch:Url"]))
                .DefaultIndex(IndexName);

            _client = new ElasticClient(settings);

            // Cria o índice se não existir
            if (!_client.Indices.Exists(IndexName).Exists)
            {
                _client.Indices.Create(IndexName, c => c
                    .Map<Game>(m => m.AutoMap())
                );
            }
        }

        public async Task IndexGameAsync(Game game)
        {
            await _client.IndexDocumentAsync(game);
        }

        public async Task<IEnumerable<Game>> SearchGamesAsync(string category)
        {
            var response = await _client.SearchAsync<Game>(s => s
                .Query(q => q.Match(m => m.Field(f => f.Categoria).Query(category)))
                .Size(3)
            );

            return response.Documents;
        }
    }
}
