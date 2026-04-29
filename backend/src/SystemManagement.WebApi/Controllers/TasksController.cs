using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemManagement.Application.Common.Models;
using SystemManagement.Application.Common.Security;
using SystemManagement.Application.DTOs.Tasks;
using SystemManagement.Application.Services;
using SystemManagement.Domain.Enums;

namespace SystemManagement.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TaskDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyCollection<TaskDto>>> GetMyTasks(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetMyTasksAsync(cancellationToken));
    }

    [HttpGet("assignable-users")]
    [Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
    public async Task<ActionResult<IReadOnlyCollection<AssignableUserDto>>> GetAssignableUsers(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAssignableUsersAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
    public async Task<ActionResult<TaskDto>> Create([FromForm] CreateTaskFormModel request, CancellationToken cancellationToken)
    {
        var dto = new CreateTaskRequest(
            request.Title,
            request.Description,
            request.AssignedToUserId,
            request.DueDate,
            request.Priority,
            request.SourceType);

        var files = await ToFileUploadsAsync(request.Files, cancellationToken);
        return Ok(await _service.CreateAsync(dto, files, cancellationToken));
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
    public async Task<ActionResult<TaskDto>> Assign(Guid id, AssignTaskRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _service.AssignAsync(id, request, cancellationToken));
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<TaskDto>> Accept(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.AcceptAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/progress")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<TaskDto>> UpdateProgress(Guid id, [FromForm] UpdateTaskProgressFormModel request, CancellationToken cancellationToken)
    {
        var dto = new UpdateTaskProgressRequest(request.ProgressPercent, request.Note);
        var files = await ToFileUploadsAsync(request.Files, cancellationToken);
        return Ok(await _service.UpdateProgressAsync(id, dto, files, cancellationToken));
    }

    [HttpPost("{id:guid}/submit-review")]
    public async Task<ActionResult<TaskDto>> SubmitReview(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.SubmitReviewAsync(id, cancellationToken));
    }

    [HttpPost("{id:guid}/complete")]
    [Consumes("multipart/form-data")]
    [Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
    public async Task<ActionResult<TaskDto>> Complete(Guid id, [FromForm] CompleteTaskFormModel request, CancellationToken cancellationToken)
    {
        var dto = new CompleteTaskRequest(request.ResultSummary);
        var files = await ToFileUploadsAsync(request.Files, cancellationToken);
        return Ok(await _service.CompleteAsync(id, dto, files, cancellationToken));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.ManagerOrAdmin)]
    public async Task<ActionResult<TaskDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _service.CancelAsync(id, cancellationToken));
    }

    private static async Task<IReadOnlyCollection<FileUploadData>> ToFileUploadsAsync(IReadOnlyCollection<IFormFile>? formFiles, CancellationToken cancellationToken)
    {
        if (formFiles is null || formFiles.Count == 0)
        {
            return Array.Empty<FileUploadData>();
        }

        var files = new List<FileUploadData>(formFiles.Count);
        foreach (var formFile in formFiles)
        {
            if (formFile.Length <= 0)
            {
                continue;
            }

            await using var stream = formFile.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, cancellationToken);
            files.Add(new FileUploadData(formFile.FileName, formFile.ContentType, formFile.Length, memory.ToArray()));
        }

        return files;
    }

    public sealed class CreateTaskFormModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid AssignedToUserId { get; set; }
        public DateTime DueDate { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Normal;
        public TaskSourceType SourceType { get; set; } = TaskSourceType.AdHoc;
        public List<IFormFile> Files { get; set; } = new();
    }

    public sealed class UpdateTaskProgressFormModel
    {
        public int ProgressPercent { get; set; }
        public string? Note { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }

    public sealed class CompleteTaskFormModel
    {
        public string? ResultSummary { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }
}
