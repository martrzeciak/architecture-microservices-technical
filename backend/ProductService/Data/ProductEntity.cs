namespace ProductService.Data;

public class ProductEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Price { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public int Stock { get; set; }
}
