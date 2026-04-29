using Microsoft.EntityFrameworkCore;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.Common.Models;
using SystemManagement.Application.DTOs.Notifications;
using SystemManagement.Application.DTOs.Tasks;
using SystemManagement.Application.Services;
using SystemManagement.Domain.Constants;
using SystemManagement.Domain.Entities;
using SystemManagement.Domain.Enums;
using SystemManagement.Infrastructure.Persistence;
using TaskStatus = SystemManagement.Domain.Enums.TaskStatus;

namespace SystemManagement.Infrastructure.Services;

public sealed class TaskService : ITaskService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDepartmentAccessService _departmentAccess;
    private readonly INotificationService _notificationService;
    private readonly IFileStorageService _fileStorageService;

    public TaskService(
        AppDbContext dbContext,
        ICurrentUserService currentUser,
        IDepartmentAccessService departmentAccess,
        INotificationService notificationService,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _departmentAccess = departmentAccess;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyCollection<TaskDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var query = BaseTaskQuery();
        var visibleDepartmentIds = await _departmentAccess.GetVisibleDepartmentIdsAsync(cancellationToken);

        if (_currentUser.RoleLevel <= RoleLevels.ChuyenVien)
        {
            var userId = RequireUserId();
            query = query.Where(x => x.AssignedToUserId == userId || x.AssignedByUserId == userId);
        }
        else
        {
            query = query.Where(x => visibleDepartmentIds.Contains(x.DepartmentId));
        }

        var tasks = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return tasks.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyCollection<TaskDto>> GetMyTasksAsync(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var tasks = await BaseTaskQuery()
            .Where(x => x.AssignedToUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
        return tasks.Select(x => x.ToDto()).ToList();
    }

    public async Task<IReadOnlyCollection<AssignableUserDto>> GetAssignableUsersAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin && _currentUser.RoleLevel < RoleLevels.PhoPhong)
        {
            return Array.Empty<AssignableUserDto>();
        }

        var currentUserId = RequireUserId();
        var visibleDepartmentIds = await _departmentAccess.GetVisibleDepartmentIdsAsync(cancellationToken);
        var currentEmployeeId = await _dbContext.Employees
            .Where(x => !x.IsDeleted && x.UserId == currentUserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var users = await _dbContext.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .Include(x => x.Employee).ThenInclude(x => x!.Department)
            .Include(x => x.Employee).ThenInclude(x => x!.ManagerEmployee)
            .Where(x => !x.IsDeleted && x.IsActive && x.Id != currentUserId && x.Employee != null && visibleDepartmentIds.Contains(x.Employee.DepartmentId))
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return users
            .Where(x =>
            {
                var role = x.UserRoles.OrderByDescending(r => r.Role.Level).Select(r => r.Role.Level).FirstOrDefault();
                if (!_currentUser.IsAdmin && role >= _currentUser.RoleLevel)
                {
                    return false;
                }

                if (_currentUser.IsAdmin || _currentUser.RoleLevel >= RoleLevels.TruongPhong)
                {
                    return true;
                }

                if (currentEmployeeId.HasValue && x.Employee?.ManagerEmployeeId == currentEmployeeId.Value)
                {
                    return true;
                }

                return _currentUser.DepartmentId.HasValue && x.Employee?.DepartmentId == _currentUser.DepartmentId.Value;
            })
            .Select(x => x.ToAssignableDto())
            .ToList();
    }

    public async Task<TaskDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await BaseTaskQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhiệm vụ.");
        await EnsureCanViewTaskAsync(task, cancellationToken);
        return task.ToDto();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, IReadOnlyCollection<FileUploadData> files, CancellationToken cancellationToken)
    {
        var currentUserId = RequireUserId();
        var assignee = await GetUserWithRoleAndDepartmentAsync(request.AssignedToUserId, cancellationToken);
        await EnsureCanAssignToUserAsync(assignee, cancellationToken);

        var departmentId = assignee.Employee?.DepartmentId ?? throw new InvalidOperationException("Người nhận việc chưa được gắn phòng ban.");

        var task = new TaskItem
        {
            TaskCode = await GenerateTaskCodeAsync(cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description,
            AssignedByUserId = currentUserId,
            AssignedToUserId = assignee.Id,
            DepartmentId = departmentId,
            DueDate = request.DueDate,
            Priority = request.Priority,
            SourceType = request.SourceType,
            Status = TaskStatus.Assigned,
            ProgressPercent = 0
        };

        task.ProgressLogs.Add(new TaskProgressLog
        {
            ProgressPercent = 0,
            Status = TaskStatus.Assigned,
            Note = "Tạo và giao nhiệm vụ.",
            ActionByUserId = currentUserId
        });

        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (files.Count > 0)
        {
            await _fileStorageService.SaveTaskFilesAsync(task.Id, null, TaskAttachmentType.AssignmentDocument, currentUserId, files, cancellationToken);
        }

        await SendAssignmentNotificationAsync(task, cancellationToken);
        return await GetByIdAsync(task.Id, cancellationToken);
    }

    public async Task<TaskDto> AssignAsync(Guid id, AssignTaskRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = RequireUserId();
        var task = await _dbContext.TaskItems.Include(x => x.ProgressLogs).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhiệm vụ.");
        await EnsureCanViewTaskAsync(task, cancellationToken);

        var assignee = await GetUserWithRoleAndDepartmentAsync(request.AssignedToUserId, cancellationToken);
        await EnsureCanAssignToUserAsync(assignee, cancellationToken);

        task.AssignedByUserId = currentUserId;
        task.AssignedToUserId = assignee.Id;
        task.DepartmentId = assignee.Employee!.DepartmentId;
        task.Status = TaskStatus.Assigned;
        task.ProgressPercent = 0;
        task.AcceptedAt = null;
        task.CompletedAt = null;
        task.ProgressLogs.Add(new TaskProgressLog
        {
            ProgressPercent = 0,
            Status = TaskStatus.Assigned,
            Note = request.Note ?? "Giao lại nhiệm vụ.",
            ActionByUserId = currentUserId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await SendAssignmentNotificationAsync(task, cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TaskDto> AcceptAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = RequireUserId();
        var task = await _dbContext.TaskItems.Include(x => x.ProgressLogs).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhiệm vụ.");

        if (task.AssignedToUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Chỉ người được giao mới được nhận việc.");
        }

        task.Status = TaskStatus.Accepted;
        task.AcceptedAt = DateTime.UtcNow;
        task.ProgressLogs.Add(new TaskProgressLog
        {
            ProgressPercent = task.ProgressPercent,
            Status = task.Status,
            Note = "Đã tiếp nhận nhiệm vụ.",
            ActionByUserId = currentUserId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TaskDto> UpdateProgressAsync(Guid id, UpdateTaskProgressRequest request, IReadOnlyCollection<FileUploadData> files, CancellationToken cancellationToken)
    {
        if (request.ProgressPercent is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(request.ProgressPercent), "Tiến độ phải từ 0 đến 100.");
        }

        var currentUserId = RequireUserId();
        var task = await _dbContext.TaskItems.Include(x => x.ProgressLogs).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhiệm vụ.");

        var canManage = _currentUser.RoleLevel >= RoleLevels.PhoPhong && await _departmentAccess.CanAccessDepartmentAsync(task.DepartmentId, cancellationToken);
        if (task.AssignedToUserId != currentUserId && !canManage)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền cập nhật nhiệm vụ này.");
        }

        task.ProgressPercent = request.ProgressPercent;
        task.Status = request.ProgressPercent >= 100 ? TaskStatus.PendingReview : TaskStatus.InProgress;

        var log = new TaskProgressLog
        {
            ProgressPercent = task.ProgressPercent,
            Status = task.Status,
            Note = request.Note,
            ActionByUserId = currentUserId
        };
        task.ProgressLogs.Add(log);

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (files.Count > 0)
        {
            await _fileStorageService.SaveTaskFilesAsync(task.Id, log.Id, TaskAttachmentType.ProgressResult, currentUserId, files, cancellationToken);
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TaskDto> SubmitReviewAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = RequireUserId();
        var task = await _dbContext.TaskItems.Include(x => x.ProgressLogs).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhiệm vụ.");
        if (task.AssignedToUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Chỉ người được giao mới được trình duyệt nhiệm vụ.");
        }

        task.Status = TaskStatus.PendingReview;
        task.ProgressLogs.Add(new TaskProgressLog
        {
            ProgressPercent = task.ProgressPercent,
            Status = task.Status,
            Note = "Trình duyệt nhiệm vụ.",
            ActionByUserId = currentUserId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TaskDto> CompleteAsync(Guid id, CompleteTaskRequest request, IReadOnlyCollection<FileUploadData> files, CancellationToken cancellationToken)
    {
        var currentUserId = RequireUserId();
        var task = await _dbContext.TaskItems.Include(x => x.ProgressLogs).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhiệm vụ.");
        if (_currentUser.RoleLevel < RoleLevels.PhoPhong || !await _departmentAccess.CanAccessDepartmentAsync(task.DepartmentId, cancellationToken))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền hoàn thành nhiệm vụ này.");
        }

        task.Status = TaskStatus.Completed;
        task.ProgressPercent = 100;
        task.CompletedAt = DateTime.UtcNow;
        task.ResultSummary = request.ResultSummary;

        var log = new TaskProgressLog
        {
            ProgressPercent = 100,
            Status = task.Status,
            Note = string.IsNullOrWhiteSpace(request.ResultSummary) ? "Hoàn thành nhiệm vụ." : request.ResultSummary,
            ActionByUserId = currentUserId
        };
        task.ProgressLogs.Add(log);

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (files.Count > 0)
        {
            await _fileStorageService.SaveTaskFilesAsync(task.Id, log.Id, TaskAttachmentType.ProgressResult, currentUserId, files, cancellationToken);
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TaskDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = RequireUserId();
        var task = await _dbContext.TaskItems.Include(x => x.ProgressLogs).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy nhiệm vụ.");
        if (_currentUser.RoleLevel < RoleLevels.PhoPhong || !await _departmentAccess.CanAccessDepartmentAsync(task.DepartmentId, cancellationToken))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền hủy nhiệm vụ này.");
        }

        task.Status = TaskStatus.Cancelled;
        task.ProgressLogs.Add(new TaskProgressLog
        {
            ProgressPercent = task.ProgressPercent,
            Status = task.Status,
            Note = "Hủy nhiệm vụ.",
            ActionByUserId = currentUserId
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private IQueryable<TaskItem> BaseTaskQuery() => _dbContext.TaskItems
        .Include(x => x.Department)
        .Include(x => x.AssignedByUser)
        .Include(x => x.AssignedToUser)
        .Include(x => x.Files).ThenInclude(x => x.UploadedByUser)
        .Include(x => x.ProgressLogs).ThenInclude(x => x.ActionByUser)
        .Include(x => x.ProgressLogs).ThenInclude(x => x.Files).ThenInclude(x => x.UploadedByUser)
        .Where(x => !x.IsDeleted);

    private Guid RequireUserId() => _currentUser.UserId ?? throw new UnauthorizedAccessException("Chưa đăng nhập.");

    private async Task<User> GetUserWithRoleAndDepartmentAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Include(x => x.UserRoles).ThenInclude(x => x.Role)
            .Include(x => x.Employee).ThenInclude(x => x!.Department)
            .Include(x => x.Employee).ThenInclude(x => x!.ManagerEmployee)
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng nhận việc.");
    }

    private async Task EnsureCanAssignToUserAsync(User assignee, CancellationToken cancellationToken)
    {
        if (assignee.Employee is null)
        {
            throw new InvalidOperationException("Người nhận việc chưa có hồ sơ nhân sự/phòng ban.");
        }

        if (!await _departmentAccess.CanAccessDepartmentAsync(assignee.Employee.DepartmentId, cancellationToken))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền giao việc cho phòng ban này.");
        }

        var assigneeLevel = assignee.UserRoles.OrderByDescending(x => x.Role.Level).Select(x => x.Role.Level).FirstOrDefault();
        if (!_currentUser.IsAdmin && _currentUser.RoleLevel <= assigneeLevel)
        {
            throw new UnauthorizedAccessException("Chỉ được giao việc cho người có role thấp hơn.");
        }

        if (_currentUser.IsAdmin || _currentUser.RoleLevel >= RoleLevels.TruongPhong)
        {
            return;
        }

        var currentEmployeeId = await _dbContext.Employees
            .Where(x => !x.IsDeleted && x.UserId == _currentUser.UserId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!currentEmployeeId.HasValue || assignee.Employee.ManagerEmployeeId != currentEmployeeId.Value)
        {
            throw new UnauthorizedAccessException("Bạn chỉ được giao việc cho nhân viên cấp dưới trực tiếp.");
        }
    }

    private async Task EnsureCanViewTaskAsync(TaskItem task, CancellationToken cancellationToken)
    {
        var currentUserId = RequireUserId();
        if (_currentUser.RoleLevel <= RoleLevels.ChuyenVien && task.AssignedToUserId != currentUserId && task.AssignedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem nhiệm vụ này.");
        }

        if (_currentUser.RoleLevel > RoleLevels.ChuyenVien && !await _departmentAccess.CanAccessDepartmentAsync(task.DepartmentId, cancellationToken))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem nhiệm vụ thuộc phòng ban này.");
        }
    }

    private async Task<string> GenerateTaskCodeAsync(CancellationToken cancellationToken)
    {
        var total = await _dbContext.TaskItems.CountAsync(cancellationToken) + 1;
        return $"TASK-{DateTime.UtcNow:yyyyMMdd}-{total:D4}";
    }

    private async Task SendAssignmentNotificationAsync(TaskItem task, CancellationToken cancellationToken)
    {
        var assigneeName = await _dbContext.Users.Where(x => x.Id == task.AssignedToUserId).Select(x => x.FullName).FirstAsync(cancellationToken);
        await _notificationService.SendAsync(new CreateNotificationRequest(
            task.AssignedToUserId,
            "TaskAssigned",
            "Bạn được giao nhiệm vụ mới",
            $"Nhiệm vụ '{task.Title}' đã được giao cho {assigneeName}. Hạn xử lý: {task.DueDate:dd/MM/yyyy HH:mm}.",
            task.Id,
            nameof(TaskItem)), cancellationToken);
    }
}
