using FileUploadAndReport.Demo.Api.Contracts.Requests;

namespace FileUploadAndReport.Demo.Api.Repositories;

public interface IFileUploadRepository
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<int> AddAsync(
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FileUploadRecord>> GetAsync(
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
