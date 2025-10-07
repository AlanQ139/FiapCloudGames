namespace GameService.DTOs
{
    public class CategoryMetric
    {
        public string Categoria { get; set; } = string.Empty;
        public long Quantidade { get; set; }
    }

    public class GameMetricsDto
    {
        public IEnumerable<CategoryMetric> PorCategoria { get; set; } = Enumerable.Empty<CategoryMetric>();
        public double? PrecoMedio { get; set; }
    }
}
