using FileUploadAndReport.Demo.Api.Authentication;
using FileUploadAndReport.Demo.Api.Configuration;
using FileUploadAndReport.Demo.Api.Repositories;
using FileUploadAndReport.Demo.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi;

LocalEnvironmentFile.Load(".env.local");
var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrWhiteSpace(builder.Configuration["FILE_UPLOAD_API_KEY"]))
{
    throw new InvalidOperationException("The FILE_UPLOAD_API_KEY environment variable is required.");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        ApiKeyAuthenticationDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Name = ApiKeyAuthenticationDefaults.HeaderName,
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Description = "Enter the API key from FILE_UPLOAD_API_KEY."
        });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            document)] = []
    });
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
    })
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationDefaults.AuthenticationScheme,
        _ => { });
builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("FileUploads")
    ?? throw new InvalidOperationException("The FileUploads connection string is required.");

// singleton because no request state stored, no connection kept open, each operation has its own transaction
builder.Services.AddSingleton<IFileUploadRepository>(_ =>
    new SqliteFileUploadRepository(connectionString));
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

var app = builder.Build();

// initializes the database and creates the table if it does not exist
await app.Services
    .GetRequiredService<IFileUploadRepository>()
    .InitializeAsync();

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
