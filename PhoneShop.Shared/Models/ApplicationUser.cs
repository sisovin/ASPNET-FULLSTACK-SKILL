namespace PhoneShop.Shared.Models;

public class ApplicationUser
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Role { get; set; } = "User"; // "User" | "Admin"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
