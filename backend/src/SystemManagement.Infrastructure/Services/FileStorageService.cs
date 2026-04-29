using Microsoft.AspNetCore.Hosting;
using SystemManagement.Application.Common.Interfaces;
using SystemManagement.Application.Common.Models;
using SystemManagement.Domain.Entities;
using SystemManagement.Domain.Enums;
using SystemManagement.Infrastructure.Persistence;

namespace SystemManagement.Infrastructure.Services;

public sealed class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _dbContext;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".png", ".jpg", ".jpeg", ".zip", ".rar", ".txt"
    };

    public FileStorageService(IWebHostEnvironment environment, AppDbContext dbContext)
    {
        _environment = environment;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TaskFile>> SaveTaskFilesAsync(
        Guid taskId,
        Guid? taskProgressLogId,
        TaskAttachmentType attachmentType,
        Guid uploadedByUserId,
        IReadOnlyCollection<FileUploadData> files,
        CancellationToken cancellationToken = default)
    {
        if (files.Count == 0)
        {
            return Array.Empty<TaskFile>();
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var phaseFolder = attachmentType == TaskAttachmentType.AssignmentDocument ? "assignment" : "progress";
        var targetFolder = Path.Combine(webRoot, "uploads", "tasks", taskId.ToString("N"), phaseFolder);
        Directory.CreateDirectory(targetFolder);

        var entities = new List<TaskFile>();
        foreach (var file in files)
        {
            if (file.Length <= 0)
            {
                continue;
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Định dạng file không được hỗ trợ: {extension}");
            }

            var safeName = Path.GetFileName(file.FileName);
            var storedName = $"{Guid.NewGuid():N}{extension}";
            var absolutePath = Path.Combine(targetFolder, storedName);
            await File.WriteAllBytesAsync(absolutePath, file.Content, cancellationToken);

            var relativePath = $"/uploads/tasks/{taskId:N}/{phaseFolder}/{storedName}";

            var entity = new TaskFile
            {
                TaskItemId = taskId,
                TaskProgressLogId = taskProgressLogId,
                AttachmentType = attachmentType,
                FileName = safeName,
                StoredFileName = storedName,
                RelativePath = relativePath,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                SizeBytes = file.Length,
                UploadedByUserId = uploadedByUserId
            };

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            _dbContext.TaskFiles.AddRange(entities);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return entities;
    }
}
