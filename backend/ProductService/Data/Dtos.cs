namespace ProductService.Data;

public record CreateProductDto(string Name, string Description, double Price, string CategoryId, int Stock);
