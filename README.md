# Full-Stack ASP.NET 10 Phone Shop Web App

A modern full-stack sample e-commerce application for selling phones and accessories.

This project uses:

- Blazor WebAssembly for the client UI
- ASP.NET Core Web API for backend services
- EF Core + MySQL for persistence
- JWT bearer authentication with role-based authorization

It is designed to run locally on Windows 11 with VS Code.

## Table of Contents

- [1) Project Goals](#1-project-goals)
- [2) Architecture](#2-architecture)
- [3) Tech Stack](#3-tech-stack)
- [4) Features](#4-features)
- [5) Repository Structure](#5-repository-structure)
- [6) Data Model Overview](#6-data-model-overview)
- [7) API Overview](#7-api-overview)
- [8) Authentication and Authorization](#8-authentication-and-authorization)
- [9) Configuration](#9-configuration)
- [10) HOW-TO-RUN in VS Code on Windows 11](#10-how-to-run-in-vs-code-on-windows-11)
- [11) Seeded Data](#11-seeded-data)
- [12) Common Issues and Fixes](#12-common-issues-and-fixes)
- [13) Useful Commands](#13-useful-commands)

## 1) Project Goals

The app supports two user roles:

- User:
	- Browse categories and products
	- Search products
	- View product details
	- Register/login/logout
	- Add/update/remove cart items
- Admin:
	- Manage categories
	- Manage products

## 2) Architecture

This solution follows a classic Client + API + Shared contract architecture:

- PhoneShop.Client (Blazor WebAssembly)
- PhoneShop.Server (ASP.NET Core Web API)
- PhoneShop.Shared (shared models, DTOs, options)

Conceptual flow:

1. Client calls API endpoints over HTTPS.
2. API validates JWT tokens and enforces role policies.
3. API reads/writes MySQL via EF Core.
4. Shared project keeps DTO/model contracts consistent across client/server.

## 3) Tech Stack

- .NET SDK: 10.0
- Frontend: Blazor WebAssembly
- Backend: ASP.NET Core Web API
- ORM: Entity Framework Core
- Database: MySQL (Pomelo provider)
- Auth: JWT Bearer Tokens
- API docs: OpenAPI + Swagger UI

## 4) Features

- Public browsing of categories and products
- Search support in product listing
- Product detail pages
- JWT-based authentication and persisted client session
- Cart operations for authenticated users
- Role-protected admin endpoints and admin pages
- Startup database migration + development data seeding

## 5) Repository Structure

Top-level tree (trimmed to important app files):

```text
PhoneShopWebapp/
|-- PhoneShop.Client/
|   |-- Auth/
|   |-- Pages/
|   |   |-- Admin/
|   |-- Services/
|   |-- wwwroot/
|   |   `-- appsettings.json
|   |-- App.razor
|   `-- Program.cs
|-- PhoneShop.Server/
|   |-- Controllers/
|   |-- Data/
|   |-- Services/
|   |-- appsettings.json
|   |-- appsettings.Development.json
|   `-- Program.cs
|-- PhoneShop.Shared/
|   |-- DTOs/
|   |-- Models/
|   `-- Options/
|-- Technical-Design-Document.md
|-- setup.ps1
`-- README.md
```

## 6) Data Model Overview

Core entities:

- ApplicationUser
- Category
- Product
- CartItem

Important DTOs:

- RegisterRequest, LoginRequest, AuthResponse
- CategoryDto, ProductDto
- CartRequest, CartItemDto

## 7) API Overview

Base path: /api

Auth:

- POST /api/auth/register
- POST /api/auth/login

Categories:

- GET /api/categories
- GET /api/categories/{id}
- POST /api/categories (Admin)
- PUT /api/categories/{id} (Admin)
- DELETE /api/categories/{id} (Admin, soft deactivate)

Products:

- GET /api/products?categoryId=&search=
- GET /api/products/{id}
- POST /api/products (Admin)
- PUT /api/products/{id} (Admin)
- DELETE /api/products/{id} (Admin, soft deactivate)

Cart (Authorized):

- GET /api/cart
- POST /api/cart/add
- POST /api/cart/update
- DELETE /api/cart/remove/{productId}
- DELETE /api/cart/clear

OpenAPI endpoint (development):

- /openapi/v1.json
- Swagger UI available at /swagger

## 8) Authentication and Authorization

- JWT config is loaded from the Jwt section in server settings.
- Claims include user id (sub), email, name, and role.
- Policy AdminOnly is used to protect admin operations.
- Client stores token in local storage and attaches Bearer auth header.

## 9) Configuration

Server settings:

- ConnectionStrings:DefaultConnection (MySQL connection)
- Jwt:Key, Issuer, Audience, ExpiresMinutes
- AllowedOrigins (must include the client origin)

Client settings:

- ApiBaseUrl in PhoneShop.Client/wwwroot/appsettings.json

Default development values:

- API base URL: https://localhost:5001
- Allowed client origin: https://localhost:7001

If your client runs on another port, update AllowedOrigins.

## 10) HOW-TO-RUN in VS Code on Windows 11

### Prerequisites

Install:

1. .NET SDK 10
2. MySQL Server 8+ (or compatible)
3. VS Code
4. VS Code C# extension (recommended: C# Dev Kit)

Optional but recommended:

- EF CLI tool:

```powershell
dotnet tool install --global dotnet-ef
```

Verify tools:

```powershell
dotnet --version
mysql --version
```

### Step A: Open project in VS Code

```powershell
cd d:\laragon\www\PhoneShopWebapp
code .
```

### Step B: Bootstrap solution (if needed)

If PhoneShop.sln does not exist yet, run:

```powershell
pwsh -ExecutionPolicy Bypass -File .\setup.ps1
```

This creates the solution and adds all 3 projects.

### Step C: Configure database and JWT

Edit PhoneShop.Server/appsettings.Development.json:

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Server=localhost;Database=phoneshop;User=root;Password=yourpassword;TreatTinyAsBoolean=true;"
	},
	"Jwt": {
		"Key": "dev-secret-key-change-me-min32chars!!",
		"Issuer": "PhoneShop",
		"Audience": "PhoneShopClient",
		"ExpiresMinutes": 60
	},
	"AllowedOrigins": "https://localhost:7001"
}
```

Also ensure client API target is correct in PhoneShop.Client/wwwroot/appsettings.json:

```json
{
	"ApiBaseUrl": "https://localhost:5001"
}
```

### Step D: Create database and migrations

Create an empty MySQL database named phoneshop, then run:

```powershell
cd .\PhoneShop.Server
dotnet ef migrations add InitialCreate -o Data/Migrations
dotnet ef database update
cd ..
```

Note: The server also executes Database.Migrate() on startup.

### Step E: Trust HTTPS development certificate

```powershell
dotnet dev-certs https --trust
```

### Step F: Run backend and frontend in two VS Code terminals

Terminal 1 (API):

```powershell
dotnet run --project .\PhoneShop.Server
```

Terminal 2 (Client):

```powershell
dotnet run --project .\PhoneShop.Client
```

### Step G: Open app and API docs

- Client app: use the HTTPS URL printed by the client terminal (typically https://localhost:7001)
- API Swagger: https://localhost:5001/swagger

If client port is not 7001, update AllowedOrigins in server settings.

### Step H: Login with seeded admin account

- Email: admin@phoneshop.local
- Password: Admin1234!

## 11) Seeded Data

On first run, seed logic creates:

- 1 admin user
- Categories:
	- Android
	- iOS
	- Accessories
- Sample products:
	- Samsung Galaxy S24
	- Google Pixel 9
	- iPhone 16
	- USB-C Fast Charger 65W

## 12) Common Issues and Fixes

1. CORS error in browser
	 - Ensure AllowedOrigins matches the actual client HTTPS URL.

2. MySQL connection failure
	 - Verify host/user/password in DefaultConnection.
	 - Ensure MySQL service is running.

3. Migration errors
	 - Run dotnet restore first.
	 - Ensure dotnet-ef is installed and available in PATH.

4. 401 Unauthorized after login
	 - Check JWT Key/Issuer/Audience values are consistent.
	 - Clear browser local storage token and login again.

5. HTTPS certificate warnings
	 - Run dotnet dev-certs https --trust and restart browser.

## 13) Useful Commands

Restore/build all:

```powershell
dotnet restore
dotnet build
```

Run only API:

```powershell
dotnet run --project .\PhoneShop.Server
```

Run only Client:

```powershell
dotnet run --project .\PhoneShop.Client
```

Create/update migrations:

```powershell
cd .\PhoneShop.Server
dotnet ef migrations add <MigrationName> -o Data/Migrations
dotnet ef database update
```