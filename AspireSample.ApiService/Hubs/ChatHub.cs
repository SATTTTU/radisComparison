using Microsoft.AspNetCore.SignalR;

namespace AspireSample.ApiService.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinRoom(string room, string user)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        await Clients.Group(room).SendAsync("ReceiveMessage", "System", $"{user} joined {room}");
    }

    public async Task SendToRoom(string room, string user, string msg)
    {
        await Clients.Group(room).SendAsync("ReceiveMessage", user, msg);
    }
}
