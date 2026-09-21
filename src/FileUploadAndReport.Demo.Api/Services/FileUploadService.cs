using FileUploadAndReport.Demo.Api.Contracts.Requests;
using FileUploadAndReport.Demo.Api.Contracts.Responses;
using FileUploadAndReport.Demo.Api.Repositories;

namespace FileUploadAndReport.Demo.Api.Services;

public sealed class FileUploadService : IFileUploadService
{
    private readonly IFileUploadRepository _repository;

    public FileUploadService(IFileUploadRepository repository)
    {
        _repository = repository;
    }

    public async Task<FileUploadResponse> UploadAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var count = await _repository.AddAsync(request, cancellationToken);

        return new FileUploadResponse
        {
            Message = request.Message!,
            FilesUploadedCounter = count
        };
    }
}
