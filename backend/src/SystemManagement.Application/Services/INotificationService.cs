using SystemManagement.Application.DTOs.Notifications;

namespace SystemManagement.Application.Services;

public interface INotificationService
{
    Task<IReadOnlyCollection<NotificationDto>> GetMyAsync(int take, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken);
    Task SendAsync(CreateNotificationRequest request, CancellationToken cancellationToken);
}
