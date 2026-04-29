using SystemManagement.Application.Common.Models;
using SystemManagement.Domain.Entities;
using SystemManagement.Domain.Enums;

namespace SystemManagement.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<IReadOnlyCollection<TaskFile>> SaveTaskFilesAsync(
        Guid taskId,
        Guid? taskProgressLogId,
        TaskAttachmentType attachmentType,
        Guid uploadedByUserId,
        IReadOnlyCollection<FileUploadData> files,
        CancellationToken cancellationToken = default);
}
