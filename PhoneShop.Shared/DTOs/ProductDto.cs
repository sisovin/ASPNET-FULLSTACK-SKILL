using System.ComponentModel.DataAnnotations;

namespace PhoneShop.Shared.DTOs;

public class ProductDto
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    [Range(0.01, (double)decimal.MaxValue)]
    public decimal Price { get; set; }

    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }

    [Required]
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
}
