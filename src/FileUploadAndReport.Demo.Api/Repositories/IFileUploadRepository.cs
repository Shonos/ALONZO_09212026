namespace FileUploadAndReport.Demo.Api.Repositories;

public interface IFileUploadRepository
{
    Task TrackAsync(string fileName, CancellationToken cancellationToken = default);
}
