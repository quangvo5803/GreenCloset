namespace DataAccess.Models
{
    public class ProductFilter
    {
        public string? Search { get; set; }
        public List<int>? CategoryIds { get; set; }
        public List<ProductColor>? Colors { get; set; }
        public List<SizeClother>? ClotherSizes { get; set; }
        public List<int>? ShoeSizes { get; set; }
        public double? PriceFrom { get; set; }
        public double? PriceTo { get; set; }
    }
}
