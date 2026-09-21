namespace FileUploadAndReport.Demo.Api.Repositories;

public sealed class DuplicateEventIdException : Exception
{
    public DuplicateEventIdException(string eventId, Exception innerException)
        : base($"An upload with EventId '{eventId}' already exists.", innerException)
    {
        EventId = eventId;
    }

    public string EventId { get; }
}
