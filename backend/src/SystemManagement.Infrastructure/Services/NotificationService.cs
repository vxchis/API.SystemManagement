using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.DTOs.Notifications;
using SystemManagement.Application.Services;
using SystemManagement.Domain.Entities;
using SystemManagement.Infrastructure.Persistence;
using SystemManagement.Infrastructure.Realtime;

namespace SystemManagement.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(AppDbContext dbContext, ICurrentUserService currentUser, IHubContext<NotificationHub> hubContext)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _hubContext = hubContext;
    }

    public async Task<IReadOnlyCollection<NotificationDto>> GetMyAsync(int take, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Chưa đăng nhập.");
        var notifications = await _dbContext.Notifications
            .Where(x => !x.IsDeleted && x.TargetUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(cancellationToken);
        return notifications.Select(x => x.ToDto()).ToList();
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Chưa đăng nhập.");
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.TargetUserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy thông báo.");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SendAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            TargetUserId = request.TargetUserId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            RelatedEntityId = request.RelatedEntityId,
            RelatedEntityType = request.RelatedEntityType,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = notification.ToDto();
        await _hubContext.Clients.User(request.TargetUserId.ToString())
            .SendAsync("notificationReceived", dto, cancellationToken);
    }
}
