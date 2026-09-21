using System.ComponentModel.DataAnnotations;

namespace FileUploadAndReport.Demo.Api.Contracts.Requests;

public sealed class FileUploadRequest : IValidatableObject
{
    [Required]
    public string? EventId { get; init; }

    [Required]
    [EnumDataType(typeof(FileUploadEventType))]
    public FileUploadEventType? EventType { get; init; }

    [Required]
    public string? Message { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EventId))
        {
            yield return new ValidationResult(
                "EventId is required.",
                new[] { nameof(EventId) });
        }

        if (string.IsNullOrWhiteSpace(Message))
        {
            yield return new ValidationResult(
                "Message is required.",
                new[] { nameof(Message) });
        }
    }
}

public enum FileUploadEventType
{
    OneAndOnlyEventType
}
