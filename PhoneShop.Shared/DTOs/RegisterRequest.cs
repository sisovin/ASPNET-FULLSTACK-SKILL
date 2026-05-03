using System.ComponentModel.DataAnnotations;

namespace PhoneShop.Shared.DTOs;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required, MinLength(6)]
    public string Password { get; set; } = default!;

    [Required]
    public string FullName { get; set; } = default!;
}
