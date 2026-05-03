using System.ComponentModel.DataAnnotations;

namespace PhoneShop.Shared.DTOs;

public class CategoryDto
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }
}
