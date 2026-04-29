using SystemManagement.Domain.Common;
using SystemManagement.Domain.Enums;
using DomainTaskStatus = SystemManagement.Domain.Enums.TaskStatus;

namespace SystemManagement.Domain.Entities;

public sealed class TaskProgressLog : AuditableEntity
{
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = default!;
    public int ProgressPercent { get; set; }
    public DomainTaskStatus Status { get; set; }
    public string? Note { get; set; }
    public Guid ActionByUserId { get; set; }
    public User ActionByUser { get; set; } = default!;
    public ICollection<TaskFile> Files { get; set; } = new List<TaskFile>();
}
