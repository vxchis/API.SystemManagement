using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SystemManagement.Infrastructure.Realtime;

[Authorize]
public sealed class NotificationHub : Hub
{
}
