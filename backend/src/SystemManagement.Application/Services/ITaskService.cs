using SystemManagement.Application.Common.Models;
using SystemManagement.Application.DTOs.Tasks;

namespace SystemManagement.Application.Services;

public interface ITaskService
{
    Task<IReadOnlyCollection<TaskDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TaskDto>> GetMyTasksAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AssignableUserDto>> GetAssignableUsersAsync(CancellationToken cancellationToken);
    Task<TaskDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, IReadOnlyCollection<FileUploadData> files, CancellationToken cancellationToken);
    Task<TaskDto> AssignAsync(Guid id, AssignTaskRequest request, CancellationToken cancellationToken);
    Task<TaskDto> AcceptAsync(Guid id, CancellationToken cancellationToken);
    Task<TaskDto> UpdateProgressAsync(Guid id, UpdateTaskProgressRequest request, IReadOnlyCollection<FileUploadData> files, CancellationToken cancellationToken);
    Task<TaskDto> SubmitReviewAsync(Guid id, CancellationToken cancellationToken);
    Task<TaskDto> CompleteAsync(Guid id, CompleteTaskRequest request, IReadOnlyCollection<FileUploadData> files, CancellationToken cancellationToken);
    Task<TaskDto> CancelAsync(Guid id, CancellationToken cancellationToken);
}
