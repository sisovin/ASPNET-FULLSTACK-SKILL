using PhoneShop.Shared.DTOs;

namespace PhoneShop.Client.Services;

public interface ICartService
{
    Task<List<CartItemDto>> GetCartAsync();
    Task AddToCartAsync(int productId, int quantity);
    Task UpdateCartAsync(int productId, int quantity);
    Task RemoveFromCartAsync(int productId);
    Task ClearCartAsync();
}
