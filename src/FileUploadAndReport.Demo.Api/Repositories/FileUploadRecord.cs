namespace FileUploadAndReport.Demo.Api.Repositories;

public sealed record FileUploadRecord(
    long Id,
    string EventId,
    string EventType,
    string Message,
    DateTimeOffset CreatedAtUtc);
