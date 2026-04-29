namespace SystemManagement.Application.Common.Models;

public sealed record FileUploadData(
    string FileName,
    string ContentType,
    long Length,
    byte[] Content);
