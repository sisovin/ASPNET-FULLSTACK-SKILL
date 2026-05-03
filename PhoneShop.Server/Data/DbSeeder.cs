using Microsoft.AspNetCore.Identity;
using PhoneShop.Shared.Models;

namespace PhoneShop.Server.Data;

public static class DbSeeder
{
    public static void Seed(PhoneShopDbContext db, IPasswordHasher<ApplicationUser> hasher)
    {
        if (db.Users.Any()) return;

        // Admin user
        var admin = new ApplicationUser
        {
            Email     = "admin@phoneshop.local",
            FullName  = "Admin",
            Role      = "Admin",
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin1234!");
        db.Users.Add(admin);

        // Categories
        var categories = new[]
        {
            new Category { Name = "Android",     Description = "Android smartphones",  IsActive = true },
            new Category { Name = "iOS",          Description = "Apple iPhones",        IsActive = true },
            new Category { Name = "Accessories",  Description = "Phone accessories",    IsActive = true }
        };
        db.Categories.AddRange(categories);
        db.SaveChanges(); // flush so category IDs are populated

        // Sample products
        db.Products.AddRange(
            new Product
            {
                Name        = "Samsung Galaxy S24",
                Slug        = "samsung-galaxy-s24",
                Description = "Flagship Android phone by Samsung with AI features.",
                Price       = 799.99m,
                Brand       = "Samsung",
                IsActive    = true,
                CategoryId  = categories[0].Id
            },
            new Product
            {
                Name        = "Google Pixel 9",
                Slug        = "google-pixel-9",
                Description = "Pure Android experience with the best camera on Android.",
                Price       = 699.99m,
                Brand       = "Google",
                IsActive    = true,
                CategoryId  = categories[0].Id
            },
            new Product
            {
                Name        = "iPhone 16",
                Slug        = "iphone-16",
                Description = "Latest Apple iPhone with the A18 Bionic chip.",
                Price       = 999.99m,
                Brand       = "Apple",
                IsActive    = true,
                CategoryId  = categories[1].Id
            },
            new Product
            {
                Name        = "USB-C Fast Charger 65W",
                Slug        = "usb-c-fast-charger-65w",
                Description = "Universal 65 W GaN fast-charging adapter.",
                Price       = 29.99m,
                Brand       = "Anker",
                IsActive    = true,
                CategoryId  = categories[2].Id
            }
        );
        db.SaveChanges();
    }
}
