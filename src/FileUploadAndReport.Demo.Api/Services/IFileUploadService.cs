using FileUploadAndReport.Demo.Api.Contracts.Requests;
using FileUploadAndReport.Demo.Api.Contracts.Responses;

namespace FileUploadAndReport.Demo.Api.Services;

public interface IFileUploadService
{
    Task<FileUploadResponse> UploadAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default);
}
