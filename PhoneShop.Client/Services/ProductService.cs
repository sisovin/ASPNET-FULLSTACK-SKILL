using System.Net.Http.Json;
using PhoneShop.Shared.DTOs;

namespace PhoneShop.Client.Services;

public class ProductService : IProductService
{
    private readonly HttpClient _http;

    public ProductService(HttpClient http) => _http = http;

    public async Task<List<ProductDto>> GetProductsAsync(int? categoryId = null, string? search = null)
    {
        var url   = "api/products";
        var query = new List<string>();

        if (categoryId.HasValue)               query.Add($"categoryId={categoryId}");
        if (!string.IsNullOrEmpty(search))     query.Add($"search={Uri.EscapeDataString(search)}");
        if (query.Count > 0) url += "?" + string.Join("&", query);

        return await _http.GetFromJsonAsync<List<ProductDto>>(url) ?? [];
    }

    public Task<ProductDto?> GetProductAsync(int id) =>
        _http.GetFromJsonAsync<ProductDto>($"api/products/{id}");

    public async Task<ProductDto?> CreateProductAsync(ProductDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/products", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ProductDto>()
            : null;
    }

    public async Task<bool> UpdateProductAsync(int id, ProductDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/products/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public Task DeleteProductAsync(int id) =>
        _http.DeleteAsync($"api/products/{id}");
}
