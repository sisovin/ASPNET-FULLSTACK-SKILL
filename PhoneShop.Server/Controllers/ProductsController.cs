using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShop.Server.Data;
using PhoneShop.Shared.DTOs;
using PhoneShop.Shared.Models;

namespace PhoneShop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly PhoneShopDbContext _db;

    public ProductsController(PhoneShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] int?    categoryId = null,
        [FromQuery] string? search     = null)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Brand != null && p.Brand.Contains(search)));

        var products = await query
            .Select(p => new ProductDto
            {
                Id           = p.Id,
                Name         = p.Name,
                Description  = p.Description,
                Price        = p.Price,
                Brand        = p.Brand,
                ImageUrl     = p.ImageUrl,
                CategoryId   = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => new ProductDto
            {
                Id           = p.Id,
                Name         = p.Name,
                Description  = p.Description,
                Price        = p.Price,
                Brand        = p.Brand,
                ImageUrl     = p.ImageUrl,
                CategoryId   = p.CategoryId,
                CategoryName = p.Category.Name
            })
            .FirstOrDefaultAsync();

        return product is null ? NotFound() : Ok(product);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto dto)
    {
        var category = await _db.Categories.FindAsync(dto.CategoryId);
        if (category is null) return BadRequest("Category not found.");

        var product = new Product
        {
            Name        = dto.Name,
            Slug        = dto.Name.ToLowerInvariant().Replace(' ', '-'),
            Description = dto.Description,
            Price       = dto.Price,
            Brand       = dto.Brand,
            ImageUrl    = dto.ImageUrl,
            CategoryId  = dto.CategoryId,
            IsActive    = true
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        dto.Id           = product.Id;
        dto.CategoryName = category.Name;
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, dto);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductDto dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.Name        = dto.Name;
        product.Slug        = dto.Name.ToLowerInvariant().Replace(' ', '-');
        product.Description = dto.Description;
        product.Price       = dto.Price;
        product.Brand       = dto.Brand;
        product.ImageUrl    = dto.ImageUrl;
        product.CategoryId  = dto.CategoryId;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
