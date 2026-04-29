using SystemManagement.Domain.Common;
using SystemManagement.Domain.Enums;

namespace SystemManagement.Domain.Entities;

public sealed class TaskFile : AuditableEntity
{
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = default!;

    public Guid? TaskProgressLogId { get; set; }
    public TaskProgressLog? TaskProgressLog { get; set; }

    public TaskAttachmentType AttachmentType { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    public Guid UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = default!;
}
