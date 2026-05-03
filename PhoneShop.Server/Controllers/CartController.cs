using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShop.Server.Data;
using PhoneShop.Shared.DTOs;
using PhoneShop.Shared.Models;

namespace PhoneShop.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly PhoneShopDbContext _db;

    public CartController(PhoneShopDbContext db) => _db = db;

    private int GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new InvalidOperationException("User identity claim missing.");
        return int.Parse(sub);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItemDto>>> GetCart()
    {
        var userId = GetUserId();
        var items = await _db.CartItems
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Product)
            .Select(ci => new CartItemDto
            {
                ProductId   = ci.ProductId,
                ProductName = ci.Product.Name,
                Price       = ci.Product.Price,
                Quantity    = ci.Quantity,
                ImageUrl    = ci.Product.ImageUrl
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(CartRequest request)
    {
        var userId  = GetUserId();
        var product = await _db.Products.FindAsync(request.ProductId);
        if (product is null || !product.IsActive)
            return NotFound("Product not found.");

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId);

        if (existing is not null)
            existing.Quantity += request.Quantity;
        else
            _db.CartItems.Add(new CartItem
            {
                UserId    = userId,
                ProductId = request.ProductId,
                Quantity  = request.Quantity
            });

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateCart(CartRequest request)
    {
        var userId = GetUserId();
        var item   = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId);

        if (item is null) return NotFound();

        if (request.Quantity <= 0)
            _db.CartItems.Remove(item);
        else
            item.Quantity = request.Quantity;

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("remove/{productId:int}")]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var userId = GetUserId();
        var item   = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (item is null) return NotFound();

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetUserId();
        var items  = _db.CartItems.Where(ci => ci.UserId == userId);
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
