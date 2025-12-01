using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace AspireSample.Web.Services;

public class ChatState
{
    private readonly NavigationManager _navigationManager;
    private readonly IConfiguration _configuration;

    public HubConnection? HubConnection { get; private set; }
    public string? AuthToken { get; private set; }
    public string? Username { get; private set; }
    public bool IsConnected => HubConnection?.State == HubConnectionState.Connected;

    public event Action? OnChange;

    public ChatState(NavigationManager navigationManager, IConfiguration configuration)
    {
        _navigationManager = navigationManager;
        _configuration = configuration;
    }

   public void SetUser(string username, string token)
{
    Username = username;
    AuthToken = token;
    NotifyStateChanged();
}


    public async Task ConnectAsync()
    {

        if (HubConnection != null)
            return;

        var apiUrl = _configuration["services:apiservice:http:0"]
            ?? _configuration["services:apiservice:https:0"];
        Console.WriteLine("API URL in ChatState = " + apiUrl);
        Console.WriteLine("TOKEN before connect = " + AuthToken);


        HubConnection = new HubConnectionBuilder()
            .WithUrl($"{apiUrl}/chathub", options =>
            {
                options.AccessTokenProvider = () =>
                {
                    Console.WriteLine($"[ChatState] AccessTokenProvider called. Token present: {!string.IsNullOrEmpty(AuthToken)}");
                    if (!string.IsNullOrEmpty(AuthToken))
                    {
                        Console.WriteLine($"[ChatState] Token starts with: {AuthToken.Substring(0, Math.Min(10, AuthToken.Length))}...");
                    }
                    return Task.FromResult(AuthToken);
                };
            })
            .WithAutomaticReconnect()
            .Build();

        await HubConnection.StartAsync();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
