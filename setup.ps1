#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Bootstraps the PhoneShop solution: creates the .sln file and links all three projects.
.DESCRIPTION
    Run once from the root of the repository:
        .\setup.ps1
    Then update appsettings.Development.json in PhoneShop.Server with your MySQL credentials,
    run EF migrations, and start the projects.
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Write-Host "Creating PhoneShop solution…" -ForegroundColor Cyan
dotnet new sln -n PhoneShop --force

Write-Host "Adding projects to solution…" -ForegroundColor Cyan
dotnet sln PhoneShop.sln add PhoneShop.Shared/PhoneShop.Shared.csproj
dotnet sln PhoneShop.sln add PhoneShop.Server/PhoneShop.Server.csproj
dotnet sln PhoneShop.sln add PhoneShop.Client/PhoneShop.Client.csproj

Write-Host "Restoring NuGet packages…" -ForegroundColor Cyan
dotnet restore

Write-Host ""
Write-Host "Setup complete." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Edit PhoneShop.Server/appsettings.Development.json with your MySQL credentials."
Write-Host "  2. Apply EF Core migrations:"
Write-Host "       cd PhoneShop.Server"
Write-Host "       dotnet ef migrations add InitialCreate -o Data/Migrations"
Write-Host "       dotnet ef database update"
Write-Host "  3. Start the API:    dotnet run --project PhoneShop.Server"
Write-Host "  4. Start the client: dotnet run --project PhoneShop.Client"
Write-Host "  5. OpenAPI UI:       https://localhost:5001/swagger"
