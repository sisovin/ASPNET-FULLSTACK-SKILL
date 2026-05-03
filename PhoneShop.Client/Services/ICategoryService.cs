using PhoneShop.Shared.DTOs;

namespace PhoneShop.Client.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto?> GetCategoryAsync(int id);
    Task<CategoryDto?> CreateCategoryAsync(CategoryDto dto);
    Task<bool> UpdateCategoryAsync(int id, CategoryDto dto);
    Task DeleteCategoryAsync(int id);
}
