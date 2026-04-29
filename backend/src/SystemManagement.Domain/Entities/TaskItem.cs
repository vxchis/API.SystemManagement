using SystemManagement.Domain.Common;
using SystemManagement.Domain.Enums;
using DomainTaskStatus = SystemManagement.Domain.Enums.TaskStatus;

namespace SystemManagement.Domain.Entities;

public sealed class TaskItem : AuditableEntity
{
    public string TaskCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;
    public Guid AssignedByUserId { get; set; }
    public User AssignedByUser { get; set; } = default!;
    public Guid AssignedToUserId { get; set; }
    public User AssignedToUser { get; set; } = default!;
    public DateTime DueDate { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Normal;
    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Assigned;
    public TaskSourceType SourceType { get; set; } = TaskSourceType.AdHoc;
    public int ProgressPercent { get; set; }
    public string? ResultSummary { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<TaskProgressLog> ProgressLogs { get; set; } = new List<TaskProgressLog>();
    public ICollection<TaskFile> Files { get; set; } = new List<TaskFile>();
}
