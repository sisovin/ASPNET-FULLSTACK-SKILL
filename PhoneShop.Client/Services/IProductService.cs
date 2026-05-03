using PhoneShop.Shared.DTOs;

namespace PhoneShop.Client.Services;

public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync(int? categoryId = null, string? search = null);
    Task<ProductDto?> GetProductAsync(int id);
    Task<ProductDto?> CreateProductAsync(ProductDto dto);
    Task<bool> UpdateProductAsync(int id, ProductDto dto);
    Task DeleteProductAsync(int id);
}
