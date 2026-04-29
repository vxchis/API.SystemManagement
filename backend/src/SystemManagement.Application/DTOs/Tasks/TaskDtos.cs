using SystemManagement.Domain.Enums;
using DomainTaskStatus = SystemManagement.Domain.Enums.TaskStatus;

namespace SystemManagement.Application.DTOs.Tasks;

public sealed record TaskFileDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string RelativePath,
    string DownloadUrl,
    TaskAttachmentType AttachmentType,
    Guid UploadedByUserId,
    string UploadedByName,
    DateTime CreatedAt);

public sealed record TaskProgressLogDto(
    Guid Id,
    int ProgressPercent,
    DomainTaskStatus Status,
    string? Note,
    Guid ActionByUserId,
    string ActionByName,
    DateTime CreatedAt,
    IReadOnlyCollection<TaskFileDto> Files);

public sealed record TaskDto(
    Guid Id,
    string TaskCode,
    string Title,
    string? Description,
    Guid DepartmentId,
    string DepartmentName,
    Guid AssignedByUserId,
    string AssignedByName,
    Guid AssignedToUserId,
    string AssignedToName,
    DateTime DueDate,
    TaskPriority Priority,
    DomainTaskStatus Status,
    TaskSourceType SourceType,
    int ProgressPercent,
    bool IsOverdue,
    string? ResultSummary,
    DateTime CreatedAt,
    IReadOnlyCollection<TaskFileDto> AssignmentFiles,
    IReadOnlyCollection<TaskProgressLogDto> ProgressLogs);

public sealed record AssignableUserDto(
    Guid UserId,
    string FullName,
    string Username,
    string RoleCode,
    int RoleLevel,
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? EmployeeId,
    string? EmployeeCode,
    Guid? ManagerEmployeeId,
    string? ManagerEmployeeName);

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    Guid AssignedToUserId,
    DateTime DueDate,
    TaskPriority Priority,
    TaskSourceType SourceType);

public sealed record AssignTaskRequest(Guid AssignedToUserId, string? Note);
public sealed record UpdateTaskProgressRequest(int ProgressPercent, string? Note);
public sealed record CompleteTaskRequest(string? ResultSummary);
