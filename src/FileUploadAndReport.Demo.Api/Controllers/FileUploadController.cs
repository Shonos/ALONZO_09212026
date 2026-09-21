using FileUploadAndReport.Demo.Api.Contracts.Requests;
using FileUploadAndReport.Demo.Api.Contracts.Responses;
using FileUploadAndReport.Demo.Api.Repositories;
using FileUploadAndReport.Demo.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileUploadAndReport.Demo.Api.Controllers;

[ApiController]
[Route("v1/[controller]")]
public sealed class FileUploadController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;

    public FileUploadController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FileUploadResponse>> Upload(
        [Required] IFormFile? file,
        CancellationToken cancellationToken)
    {
        var request = await ReadAndValidateFileAsync(file, cancellationToken);

        if (request is null || !ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            return Ok(await _fileUploadService.UploadAsync(request, cancellationToken));
        }
        catch (DuplicateEventIdException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate event",
                Detail = "An upload with this EventId already exists.",
                Instance = HttpContext.Request.Path
            });
        }
    }

    private async Task<FileUploadRequest?> ReadAndValidateFileAsync(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            ModelState.AddModelError("file", "A file is required.");
            return null;
        }

        if (file.Length == 0)
        {
            ModelState.AddModelError("file", "The uploaded file cannot be empty.");
            return null;
        }

        FileUploadRequest? request;

        try
        {
            await using var stream = file.OpenReadStream();
            request = await JsonSerializer.DeserializeAsync<FileUploadRequest>(
                stream,
                FileUploadJsonOptions,
                cancellationToken);
        }
        catch (JsonException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The uploaded file is not a valid FileUploadRequest. Ensure it contains valid JSON, EventId, EventType, and Message.");
            return null;
        }
        catch (NotSupportedException)
        {
            ModelState.AddModelError(string.Empty, "The uploaded file format is not supported.");
            return null;
        }

        if (request is null)
        {
            ModelState.AddModelError(string.Empty, "The uploaded file must contain a JSON object.");
            return null;
        }

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);
        Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

        foreach (var validationResult in validationResults)
        {
            var memberNames = validationResult.MemberNames.Any()
                ? validationResult.MemberNames
                : new[] { string.Empty };

            foreach (var memberName in memberNames)
            {
                ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "The value is invalid.");
            }
        }

        if (!ModelState.IsValid)
        {
            return null;
        }

        return request;
    }

    private static readonly JsonSerializerOptions FileUploadJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}
