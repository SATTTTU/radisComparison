using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace AspireSample.Web.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly ChatState _chatState;

    public CustomAuthStateProvider(ChatState chatState)
    {
        _chatState = chatState;
        _chatState.OnChange += StateChanged;
    }

    private void StateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        ClaimsIdentity identity = new();

        if (!string.IsNullOrEmpty(_chatState.AuthToken) &&
            !string.IsNullOrEmpty(_chatState.Username))
        {
            identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, _chatState.Username)
            }, "Jwt");
        }

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void Dispose()
    {
        _chatState.OnChange -= StateChanged;
    }
}
