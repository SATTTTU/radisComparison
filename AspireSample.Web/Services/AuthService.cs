using System.Net.Http.Json;

namespace AspireSample.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ChatState _chatState;

    public AuthService(HttpClient httpClient, ChatState chatState)
    {
        _httpClient = httpClient;
        _chatState = chatState;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new 
        { 
            Username = username, 
            Password = password 
        });

        if (!response.IsSuccessStatusCode)
            return false;

        var token = await response.Content.ReadAsStringAsync();
        _chatState.SetUser(username, token.Trim('"'));
        return true;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", new 
        { 
            Username = username, 
            Password = password 
        });

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> TestAuthAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/test");
        if (!string.IsNullOrEmpty(_chatState.AuthToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _chatState.AuthToken);
        }
        
        var response = await _httpClient.SendAsync(request);
        Console.WriteLine($"[AuthService] TestAuthAsync status: {response.StatusCode}");
        return response.IsSuccessStatusCode;
    }
}
