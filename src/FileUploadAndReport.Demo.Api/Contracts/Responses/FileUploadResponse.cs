using System.ComponentModel;

namespace FileUploadAndReport.Demo.Api.Contracts.Responses;

public sealed class FileUploadResponse
{
    [DefaultValue("Sample message")]
    public string Message { get; init; } = "Sample message";

    [DefaultValue(1)]
    public int FilesUploadedCounter { get; init; } = 1;
}
