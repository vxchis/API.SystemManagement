namespace SystemManagement.Application.DTOs.Notifications;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Message,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    bool IsRead,
    DateTime CreatedAt);

public sealed record CreateNotificationRequest(
    Guid TargetUserId,
    string Type,
    string Title,
    string Message,
    Guid? RelatedEntityId,
    string? RelatedEntityType);
