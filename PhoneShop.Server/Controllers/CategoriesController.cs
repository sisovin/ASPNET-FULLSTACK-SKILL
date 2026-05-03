using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShop.Server.Data;
using PhoneShop.Shared.DTOs;
using PhoneShop.Shared.Models;

namespace PhoneShop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly PhoneShopDbContext _db;

    public CategoriesController(PhoneShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _db.Categories
            .Where(c => c.IsActive)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _db.Categories
            .Where(c => c.Id == id && c.IsActive)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description })
            .FirstOrDefaultAsync();

        return category is null ? NotFound() : Ok(category);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryDto dto)
    {
        var category = new Category
        {
            Name        = dto.Name,
            Description = dto.Description,
            IsActive    = true
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        dto.Id = category.Id;
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, dto);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        category.Name        = dto.Name;
        category.Description = dto.Description;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        category.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
