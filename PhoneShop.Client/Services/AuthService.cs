using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PhoneShop.Client.Auth;
using PhoneShop.Shared.DTOs;

namespace PhoneShop.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient                      _http;
    private readonly ILocalStorageService            _localStorage;
    private readonly JwtAuthenticationStateProvider  _authProvider;

    public AuthService(
        HttpClient                 http,
        ILocalStorageService       localStorage,
        AuthenticationStateProvider authProvider)
    {
        _http         = http;
        _localStorage = localStorage;
        _authProvider = (JwtAuthenticationStateProvider)authProvider;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse is null) return null;

        await PersistToken(authResponse.Token);
        return authResponse;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        if (!response.IsSuccessStatusCode) return null;

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (authResponse is null) return null;

        await PersistToken(authResponse.Token);
        return authResponse;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _http.DefaultRequestHeaders.Authorization = null;
        _authProvider.NotifyUserLogout();
    }

    private async Task PersistToken(string token)
    {
        await _localStorage.SetItemAsync("authToken", token);
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        _authProvider.NotifyUserAuthentication(token);
    }
}
