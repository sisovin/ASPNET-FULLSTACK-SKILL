using System.Net.Http.Json;
using PhoneShop.Shared.DTOs;

namespace PhoneShop.Client.Services;

public class CartService : ICartService
{
    private readonly HttpClient _http;

    public CartService(HttpClient http) => _http = http;

    public async Task<List<CartItemDto>> GetCartAsync() =>
        await _http.GetFromJsonAsync<List<CartItemDto>>("api/cart") ?? [];

    public Task AddToCartAsync(int productId, int quantity) =>
        _http.PostAsJsonAsync("api/cart/add", new CartRequest { ProductId = productId, Quantity = quantity });

    public Task UpdateCartAsync(int productId, int quantity) =>
        _http.PostAsJsonAsync("api/cart/update", new CartRequest { ProductId = productId, Quantity = quantity });

    public Task RemoveFromCartAsync(int productId) =>
        _http.DeleteAsync($"api/cart/remove/{productId}");

    public Task ClearCartAsync() =>
        _http.DeleteAsync("api/cart/clear");
}
