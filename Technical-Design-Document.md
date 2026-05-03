# Phone shop web app – technical design document

## 1. Solution overview

**Goal:** A simple e‑commerce–style phone shop:

- **Users:**  
  - Browse phones by category  
  - View product details  
  - Add/remove items in a cart  
  - Register, log in, manage their own account  
- **Admin/Manager:**  
  - Manage categories  
  - Manage products  
- **Tech stack:**  
  - **Frontend:** Blazor WebAssembly  
  - **Backend:** ASP.NET Core Web API  
  - **Shared:** .NET class library for data models & DTOs  
  - **Database:** MySQL  
  - **Auth:** JWT bearer tokens (access token only is enough for this scope)  
- **Runtime:** .NET 10 SDK, all runnable locally.

---

## 2. Solution architecture

### 2.1 Project structure

Use a **hosted Blazor WebAssembly** style solution with three main projects:

1. **PhoneShop.Shared** (Class Library)
   - Shared models (entities/DTOs/view models)
   - Enums, constants, validation attributes

2. **PhoneShop.Server** (ASP.NET Core Web API)
   - Controllers for Auth, Categories, Products, Cart, Users
   - EF Core DbContext for MySQL
   - Identity or custom user table
   - JWT token generation & validation
   - Role-based authorization (User, Admin)

3. **PhoneShop.Client** (Blazor WebAssembly)
   - Pages: Home, Categories, Product list, Product details, Cart, Login, Register, Account, Admin area
   - Services: AuthService, ProductService, CategoryService, CartService, AdminService
   - HttpClient configured with base address and JWT handler
   - AuthenticationStateProvider implementation

Optional:

4. **PhoneShop.sln** – solution file tying everything together.

---

### 2.2 High-level architecture diagram (conceptual)

- **Client (Blazor WASM)**  
  → calls  
- **Server (ASP.NET Core API)**  
  → uses  
- **MySQL DB (EF Core)**  

Shared models are referenced by both Client and Server.

---

## 3. Data model design (shared library)

Namespace suggestion: `PhoneShop.Shared`.

### 3.1 Core entities

```csharp
public class ApplicationUser
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Role { get; set; } = "User"; // "User", "Admin"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
}

public class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 3.2 DTOs and auth models

```csharp
public class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FullName { get; set; } = default!;
}

public class LoginRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class AuthResponse
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Role { get; set; } = default!;
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
}
```

---

## 4. Database and EF Core configuration

### 4.1 MySQL provider

Use **Pomelo.EntityFrameworkCore.MySql**.

In `PhoneShop.Server`:

```bash
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.Identity
```

### 4.2 DbContext

```csharp
public class PhoneShopDbContext : DbContext
{
    public PhoneShopDbContext(DbContextOptions<PhoneShopDbContext> options)
        : base(options) { }

    public DbSet<ApplicationUser> Users { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<CartItem> CartItems { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.User)
            .WithMany()
            .HasForeignKey(ci => ci.UserId);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId);
    }
}
```

### 4.3 Connection string

In `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=phoneshop;User=root;Password=yourpassword;TreatTinyAsBoolean=true;"
  },
  "Jwt": {
    "Key": "super-secret-key-change-me",
    "Issuer": "PhoneShop",
    "Audience": "PhoneShopClient",
    "ExpiresMinutes": 60
  }
}
```

In `Program.cs` (Server):

```csharp
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PhoneShopDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

---

## 5. Authentication and authorization (JWT)

### 5.1 JWT configuration

In `Program.cs`:

```csharp
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JwtOptions>(jwtSection);

var jwtOptions = jwtSection.Get<JwtOptions>()!;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
});
```

`JwtOptions` class (in Shared or Server):

```csharp
public class JwtOptions
{
    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiresMinutes { get; set; }
}
```

### 5.2 Token generation service

```csharp
public interface ITokenService
{
    AuthResponse GenerateToken(ApplicationUser user);
}

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AuthResponse GenerateToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expires,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<ITokenService, TokenService>();
```

### 5.3 Password hashing

Use `PasswordHasher<ApplicationUser>`:

```csharp
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
```

---

## 6. API design

Base URL: `/api`.

### 6.1 AuthController

**Route:** `/api/auth`

- **POST** `/register`  
  - Request: `RegisterRequest`  
  - Response: `AuthResponse`  
  - Behavior: create user, hash password, default role `User`, return token.

- **POST** `/login`  
  - Request: `LoginRequest`  
  - Response: `AuthResponse`  
  - Behavior: validate credentials, return token.

Example:

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PhoneShopDbContext _db;
    private readonly IPasswordHasher<ApplicationUser> _hasher;
    private readonly ITokenService _tokenService;

    public AuthController(
        PhoneShopDbContext db,
        IPasswordHasher<ApplicationUser> hasher,
        ITokenService tokenService)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already registered.");

        var user = new ApplicationUser
        {
            Email = request.Email,
            FullName = request.FullName,
            Role = "User"
        };

        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);
        return Ok(token);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return Unauthorized("Invalid credentials.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credentials.");

        var token = _tokenService.GenerateToken(user);
        return Ok(token);
    }
}
```

---

### 6.2 CategoriesController

**Route:** `/api/categories`

- **GET** `/` – list all active categories (public)
- **GET** `/{id}` – get category by id (public)
- **POST** `/` – create category (**AdminOnly**)
- **PUT** `/{id}` – update category (**AdminOnly**)
- **DELETE** `/{id}` – soft delete or deactivate (**AdminOnly**)

Use `CategoryDto` for responses.

---

### 6.3 ProductsController

**Route:** `/api/products`

- **GET** `/` – list products (optional query: `categoryId`, `search`)
- **GET** `/{id}` – product details
- **POST** `/` – create product (**AdminOnly**)
- **PUT** `/{id}` – update product (**AdminOnly**)
- **DELETE** `/{id}` – deactivate (**AdminOnly**)

---

### 6.4 CartController

**Route:** `/api/cart` (requires authenticated user)

- **GET** `/` – get current user’s cart items
- **POST** `/add` – add item to cart  
  - Body: `{ productId, quantity }`
- **POST** `/update` – update quantity  
  - Body: `{ productId, quantity }`
- **DELETE** `/remove/{productId}` – remove item
- **DELETE** `/clear` – clear cart

Example snippet:

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly PhoneShopDbContext _db;

    public CartController(PhoneShopDbContext db)
    {
        _db = db;
    }

    private int GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
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
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                Price = ci.Product.Price,
                Quantity = ci.Quantity,
                ImageUrl = ci.Product.ImageUrl
            })
            .ToListAsync();

        return Ok(items);
    }
}
```

---

### 6.5 Admin endpoints

You can either:

- Use the same controllers with `[Authorize(Policy = "AdminOnly")]` on admin actions, or
- Create dedicated `AdminProductsController`, `AdminCategoriesController`.

Example:

```csharp
[Authorize(Policy = "AdminOnly")]
[HttpPost]
public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto dto) { ... }
```

---

## 7. Blazor WebAssembly client design

### 7.1 Program setup

In `PhoneShop.Client/Program.cs`:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// Auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();

await builder.Build().RunAsync();
```

### 7.2 JWT AuthenticationStateProvider

Implement a provider that:

- Stores token in `localStorage` (via JS interop or a small wrapper)
- Parses claims from JWT
- Exposes `AuthenticationState`

Skeleton:

```csharp
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    public JwtAuthenticationStateProvider(
        ILocalStorageService localStorage,
        HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }
}
```

(You’d add a small `JwtParser` helper to decode the token payload.)

### 7.3 HttpClient token attachment

- After login/register, store token in local storage.
- Set `Authorization` header on `HttpClient`.
- On 401 responses, clear token and redirect to login.

---

### 7.4 UI pages (minimal structure)

**Public:**

- `/` – Home: featured categories/products
- `/categories` – list categories
- `/category/{id}` – products by category
- `/product/{id}` – product details

**User:**

- `/cart` – view/edit cart (requires `[Authorize]`)
- `/account` – basic profile info (optional)

**Auth:**

- `/login`
- `/register`

**Admin:**

- `/admin` – dashboard
- `/admin/categories`
- `/admin/products`

Use `AuthorizeView` and route constraints:

```razor
@attribute [Authorize(Roles = "Admin")]
@page "/admin/products"
```

---

## 8. Local development and setup

### 8.1 Prerequisites

- .NET 10 SDK installed
- MySQL server running locally
- A database `phoneshop` created (or let EF create it if configured with migrations)

### 8.2 Steps

1. **Clone or create solution structure**

   - `PhoneShop.sln`
   - `PhoneShop.Shared`
   - `PhoneShop.Server`
   - `PhoneShop.Client`

2. **Configure connection string**

   - Update `appsettings.Development.json` in `PhoneShop.Server` with your MySQL credentials.

3. **Add EF Core migrations**

   From `PhoneShop.Server` project directory:

   ```bash
   dotnet ef migrations add InitialCreate -o Data/Migrations
   dotnet ef database update
   ```

4. **Seed initial data (optional)**

   In `Program.cs`, after building app, run a small seeding method:

   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       var db = scope.ServiceProvider.GetRequiredService<PhoneShopDbContext>();
       DbSeeder.Seed(db);
   }
   ```

   `DbSeeder` can create:

   - Default admin user (`admin@phoneshop.local` / some password)
   - A few categories (e.g., “Android”, “iOS”, “Accessories”)
   - Sample products.

5. **Run the solution**

   - Set `PhoneShop.Server` as startup project (if using hosted template, it will serve the client).
   - Run:

     ```bash
     dotnet run --project PhoneShop.Server
     ```

   - Open browser at `https://localhost:5001` or the printed URL.

---

## 9. UX and “smooth” feel

Even though this is a simple app, you can make it feel smooth by:

- **Client-side routing:** Blazor WASM already handles this.
- **Loading indicators:**  
  - Show spinners while fetching products/categories/cart.
- **Optimistic UI for cart:**  
  - Update cart UI immediately when user adds/removes items, then confirm with API.
- **Persisted auth:**  
  - Keep user logged in via token in local storage until expiration.
- **Error handling:**  
  - Show toast messages for API errors (e.g., invalid login, unauthorized, server errors).

---

## 10. Summary

You now have:

- A clear **project structure** (Client/Server/Shared).
- **Data models** for users, categories, products, and cart.
- A **MySQL-backed EF Core** context.
- **JWT-based authentication** with roles.
- A set of **API endpoints** for auth, browsing, cart, and admin management.
- A **Blazor WebAssembly client** design with token-based auth and protected routes.
- **Local setup steps** for running everything with .NET 10 and MySQL.

---