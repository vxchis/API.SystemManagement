using SystemManagement.Application.DTOs.DepartmentGroups;
using SystemManagement.Application.DTOs.Departments;
using SystemManagement.Application.DTOs.Employees;
using SystemManagement.Application.DTOs.Notifications;
using SystemManagement.Application.DTOs.Tasks;
using SystemManagement.Application.DTOs.Users;
using SystemManagement.Domain.Entities;
using SystemManagement.Domain.Enums;
using DomainTaskStatus = SystemManagement.Domain.Enums.TaskStatus;

namespace SystemManagement.Infrastructure.Services;

internal static class MappingExtensions
{
    public static DepartmentDto ToDto(this Department x) => new(x.Id, x.Code, x.Name, x.Description, x.IsActive);
    public static PositionDto ToDto(this Position x) => new(x.Id, x.Code, x.Name, x.IsActive);
    public static RoleDto ToDto(this Role x) => new(x.Id, x.Code, x.Name, x.Level);

    public static DepartmentGroupDto ToDto(this DepartmentGroup x)
    {
        var departments = x.Departments.Where(d => d.IsActive).Select(d => d.Department.ToDto()).ToList();
        return new DepartmentGroupDto(x.Id, x.Code, x.Name, x.Description, x.IsActive, departments);
    }

    public static EmployeeDto ToDto(this Employee x) => new(
        x.Id,
        x.EmployeeCode,
        x.FullName,
        x.DepartmentId,
        x.Department.Name,
        x.PositionId,
        x.Position.Name,
        x.UserId,
        x.Email,
        x.PhoneNumber,
        x.ManagerEmployeeId,
        x.ManagerEmployee?.FullName,
        x.IsActive);

    public static UserDto ToDto(this User x)
    {
        var role = x.UserRoles.OrderByDescending(r => r.Role.Level).Select(r => r.Role).FirstOrDefault();
        return new UserDto(
            x.Id,
            x.Username,
            x.FullName,
            x.Email,
            x.PhoneNumber,
            x.IsActive,
            role?.Code ?? string.Empty,
            role?.Level ?? 0,
            x.Employee?.DepartmentId,
            x.Employee?.Department.Name);
    }

    public static AssignableUserDto ToAssignableDto(this User x)
    {
        var role = x.UserRoles.OrderByDescending(r => r.Role.Level).Select(r => r.Role).FirstOrDefault();
        return new AssignableUserDto(
            x.Id,
            x.FullName,
            x.Username,
            role?.Code ?? string.Empty,
            role?.Level ?? 0,
            x.Employee?.DepartmentId,
            x.Employee?.Department.Name,
            x.Employee?.Id,
            x.Employee?.EmployeeCode,
            x.Employee?.ManagerEmployeeId,
            x.Employee?.ManagerEmployee?.FullName);
    }

    public static TaskFileDto ToDto(this TaskFile x) => new(
        x.Id,
        x.FileName,
        x.ContentType,
        x.SizeBytes,
        x.RelativePath,
        x.RelativePath,
        x.AttachmentType,
        x.UploadedByUserId,
        x.UploadedByUser.FullName,
        x.CreatedAt);

    public static TaskProgressLogDto ToDto(this TaskProgressLog x) => new(
        x.Id,
        x.ProgressPercent,
        x.Status,
        x.Note,
        x.ActionByUserId,
        x.ActionByUser.FullName,
        x.CreatedAt,
        x.Files.OrderBy(f => f.CreatedAt).Select(f => f.ToDto()).ToList());

    public static TaskDto ToDto(this TaskItem x)
    {
        var isOverdue = x.Status != DomainTaskStatus.Completed && x.Status != DomainTaskStatus.Cancelled && x.DueDate < DateTime.UtcNow;
        return new TaskDto(
            x.Id,
            x.TaskCode,
            x.Title,
            x.Description,
            x.DepartmentId,
            x.Department.Name,
            x.AssignedByUserId,
            x.AssignedByUser.FullName,
            x.AssignedToUserId,
            x.AssignedToUser.FullName,
            x.DueDate,
            x.Priority,
            x.Status,
            x.SourceType,
            x.ProgressPercent,
            isOverdue,
            x.ResultSummary,
            x.CreatedAt,
            x.Files.Where(f => f.AttachmentType == TaskAttachmentType.AssignmentDocument).OrderBy(f => f.CreatedAt).Select(f => f.ToDto()).ToList(),
            x.ProgressLogs.OrderByDescending(p => p.CreatedAt).Select(p => p.ToDto()).ToList());
    }

    public static NotificationDto ToDto(this Notification x) => new(
        x.Id,
        x.Type,
        x.Title,
        x.Message,
        x.RelatedEntityId,
        x.RelatedEntityType,
        x.IsRead,
        x.CreatedAt);
}
