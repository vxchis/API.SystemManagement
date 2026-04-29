using SystemManagement.Domain.Common;

namespace SystemManagement.Domain.Entities;

public sealed class Notification : AuditableEntity
{
    public Guid TargetUserId { get; set; }
    public User TargetUser { get; set; } = default!;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
