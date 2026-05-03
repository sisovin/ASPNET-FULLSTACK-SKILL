using System.Net.Http.Json;
using PhoneShop.Shared.DTOs;

namespace PhoneShop.Client.Services;

public class CategoryService : ICategoryService
{
    private readonly HttpClient _http;

    public CategoryService(HttpClient http) => _http = http;

    public async Task<List<CategoryDto>> GetCategoriesAsync() =>
        await _http.GetFromJsonAsync<List<CategoryDto>>("api/categories") ?? [];

    public Task<CategoryDto?> GetCategoryAsync(int id) =>
        _http.GetFromJsonAsync<CategoryDto>($"api/categories/{id}");

    public async Task<CategoryDto?> CreateCategoryAsync(CategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/categories", dto);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<CategoryDto>()
            : null;
    }

    public async Task<bool> UpdateCategoryAsync(int id, CategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/categories/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public Task DeleteCategoryAsync(int id) =>
        _http.DeleteAsync($"api/categories/{id}");
}
