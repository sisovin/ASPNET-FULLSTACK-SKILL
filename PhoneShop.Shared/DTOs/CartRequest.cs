using System.ComponentModel.DataAnnotations;

namespace PhoneShop.Shared.DTOs;

public class CartRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; } = 1;
}
