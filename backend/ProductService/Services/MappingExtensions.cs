using ProductService.Data;

namespace ProductService.Services;

public static class MappingExtensions
{
    public static EShop.ProductService.Product ToProto(this ProductEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Price = (double)entity.Price,
        CategoryId = entity.CategoryId,
        Stock = entity.Stock
    };
}
